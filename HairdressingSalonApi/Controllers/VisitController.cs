using AutoMapper;
using Microsoft.AspNet.Identity;
using NLog;
using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Globalization;
using System.Linq;
using System.Linq.Dynamic;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using WebApplication3.Exceptions;
using WebApplication3.Helpers;
using WebApplication3.Models;
using WebApplication3.Models.ApiBindingModels;
using WebApplication3.Repositories;
using WebApplication3.Results;

namespace WebApplication3.Controllers
{
    [RoutePrefix("api/visits")]
    public class VisitController : ApiController
    {
        private readonly IUnitOfWork _unitOfWork;
        private static Logger _logger;
        private IMapper _mapper;

        public VisitController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _logger = LogManager.GetCurrentClassLogger();
            _mapper = mapper;
        }

        [Route("")]
        [HttpPost]
        [Authorize(Roles = "Customer")]
        public async Task<IHttpActionResult> Add(AddVisitBindingModel model)
        {
            try
            {
                VisitTerm visitTerm = await _unitOfWork.VisitTermRepository.AddVisitTermOrReturnIfExists(model.StartDate);

                Visit visit = Visit.Create(model.ServiceId, visitTerm.Id, User.Identity.GetUserId(), model.StartDate);
                visit = _mapper.Map(model, visit);

                await _unitOfWork.VisitRepository.Add(visit);
                await _unitOfWork.CompleteAsync();
                var addedVisit = await _unitOfWork.VisitRepository.FindWithCustomerAndServiceAsync(visit.Id);
                var result = _mapper.Map<VisitInfoBindingModel>(addedVisit);

                return Ok(result);
            }
            catch (VisitCurrentlyReservedException e)
            {
                ModelState.AddModelError("model.StartDate", "Podana data została już zarezerwowana!");
                return BadRequest(ModelState);
            }
            catch (Exception e)
            {
                _logger.Error(e);
                return InternalServerError();
            }
        }

        [Route("")]
        [HttpDelete]
        [Authorize(Roles = "Admin, Customer")]
        public async Task<IHttpActionResult> Delete(int id)
        {
            Visit visit = null;

            try
            {
                visit = await _unitOfWork.VisitRepository.FindAsync(id);
            }
            catch (ItemNotFoundException e)
            {
                return NotFound();
            }
            catch (Exception e)
            {
                _logger.Error(e);
                return InternalServerError();
            }

            if (visit.CustomerId != User.Identity.GetUserId() && !User.IsInRole("Admin"))
            {
                return new ForbiddenActionResult();
            }

            try
            {
                _unitOfWork.VisitRepository.Remove(visit);
                await _unitOfWork.CompleteAsync();
            }
            catch (Exception e)
            {
                _logger.Error(e);
                return InternalServerError();
            }
            return Ok();
        }

        [Route("")]
        [HttpPut]
        [Authorize(Roles = "Admin, Hairdresser")]
        public async Task<IHttpActionResult> Update(UpdateVisitBindingModel model)
        {
            Visit visit = null;
            Visit result = null;

            VisitInfoBindingModel visitModel;

            try
            {
                visit = await _unitOfWork.VisitRepository.FindWithCustomerAndServiceAsync(model.Id);
                visit = _mapper.Map(model, visit);
                await _unitOfWork.VisitRepository.Update(visit);
                await _unitOfWork.CompleteAsync();
                result = await _unitOfWork.VisitRepository.FindWithCustomerAndServiceAsync(model.Id);
                visitModel = _mapper.Map<VisitInfoBindingModel>(visit);
            }
            catch (ItemNotFoundException e)
            {
                return NotFound();
            }
            catch (Exception e)
            {
                _logger.Error(e);
                return InternalServerError();
            }
            return Ok(visitModel);
        }

        [Route("{date:datetime}")]
        [HttpGet]
        [Authorize(Roles = "Admin, Hairdresser")]
        public async Task<IHttpActionResult> Visits(DateTime date)
        {
            VisitTerm result;
            try
            {
                result = await _unitOfWork.VisitTermRepository.GetByDateWithVisits(date);
            }
            catch (ItemNotFoundException e)
            {
                return NotFound();
            }
            catch (Exception e)
            {
                _logger.Error(e);
                return InternalServerError();
            }
            var mapedVisitTerm = _mapper.Map<VisitTermInfoBindingModel>(result);
            return Ok(mapedVisitTerm);
        }

        [Route("free-hours")]
        [HttpGet]
        public async Task<IHttpActionResult> FreeHours([FromUri]FreeHoursBindingModel model)
        {
            List<GetVisitTermByDateBindingModel> result = null;
            try
            {
                if (!(await _unitOfWork.VisitTermRepository.IsInDatabase(model.Date)))
                {
                    _unitOfWork.VisitTermRepository.AddByDate(model.Date);
                    await _unitOfWork.CompleteAsync();

                }
                result = await _unitOfWork.VisitTermRepository.GetFreeHours(model.Date);
            }
            catch (ItemNotFoundException e)
            {
                return NotFound();
            }
            catch (Exception e)
            {
                _logger.Error(e);
                return InternalServerError();
            }


            return Ok(result);
        }

        [HttpGet]
        [Route("user-visits", Name = "UserVisitsRoute")]
        [Authorize]
        public async Task<IHttpActionResult> UserVisits(string email, int pageNo = 1, int pageSize = 5, string sort = "startDate")
        {
            if (!(User.IsInRole("Admin") || User.IsInRole("Hairdresser") || User.Identity.GetUserName() == email))
            {
                return new ForbiddenActionResult();
            }

            List<Visit> result = null;
            PageLinkBuilder linkBuilder = null;

            try
            {
                var counter = await _unitOfWork.VisitRepository.VisitsCountAsync(email);
                result = await _unitOfWork.VisitRepository.GetByUserAsync(email, pageNo, pageSize, sort);
                linkBuilder = new PageLinkBuilder(Url, "UserVisitsRoute", new { sort = sort, email = email }, pageNo, pageSize, counter);
            }
            catch (ItemNotFoundException e)
            {
                return NotFound();
            }
            catch (ParseException e)
            {
                ModelState.AddModelError("sort", e.Message);
                return BadRequest(ModelState);
            }
            catch (Exception e)
            {
                _logger.Error(e);
                return InternalServerError();
            }

            HttpContext.Current.Response.Headers.Add("X-Pagination",
                Newtonsoft.Json.JsonConvert.SerializeObject(linkBuilder));
            var model = _mapper.Map<List<UserVisitInfoBindingModel>>(result);

            return Ok(model);
        }

    }
}