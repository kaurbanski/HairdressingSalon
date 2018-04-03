using AutoMapper;
using NLog;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Linq;
using System.Linq.Dynamic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using WebApplication3.Constants;
using WebApplication3.Exceptions;
using WebApplication3.Models;
using WebApplication3.Models.ApiBindingModels;
using WebApplication3.Repositories;

namespace WebApplication3.Controllers
{
    [RoutePrefix("api/services")]
    public class ServicesController : ApiController
    {
        private static Logger _logger;
        private IUnitOfWork _unitOfWork;
        private IMapper _mapper;

        public ServicesController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _logger = LogManager.GetCurrentClassLogger();
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        [Route()]
        [HttpGet]
        public async Task<IHttpActionResult> Services(string sort = "name")
        {
            IEnumerable<Service> services;

            try
            {
                services = await _unitOfWork.ServiceRepository.GetAllAsync(sort);
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
            var result = _mapper.Map<IEnumerable<ServiceBindingModel>>(services);
            return Ok(result);
        }

        [Route()]
        [HttpPut]
        [Authorize(Roles = "Admin")]
        public async Task<IHttpActionResult> Update(ServiceBindingModel model)
        {
            Service service = _mapper.Map<Service>(model);

            try
            {
                await _unitOfWork.ServiceRepository.UpdateAsync(service);
                await _unitOfWork.CompleteAsync();
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
            return Content(HttpStatusCode.Accepted, model);
        }

        [Route()]
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IHttpActionResult> Add(ServiceBindingModel model)
        {
            Service service = _mapper.Map<Service>(model);
            int result;

            try
            {
                _unitOfWork.ServiceRepository.Add(service);
                result = await _unitOfWork.CompleteAsync();
            }
            catch (Exception e)
            {
                _logger.Error(e);
                return InternalServerError();
            }

            var mapedEntity = _mapper.Map<ServiceBindingModel>(service);
            return Created("Created successfull", mapedEntity);
        }


        [Route()]
        [HttpDelete]
        [Authorize(Roles = "Admin")]
        public async Task<IHttpActionResult> Delete(int id)
        {
            int result;
            if (string.IsNullOrEmpty(id.ToString()))
            {
                return BadRequest();
            }
            try
            {
                await _unitOfWork.ServiceRepository.RemoveById(id);
                result = await _unitOfWork.CompleteAsync();
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

            return Ok();
        }

        [HttpGet]
        [Route("all-with-stats")]
        [Authorize]
        public async Task<IHttpActionResult> GetAllWithStats(string sort = "name")
        {
            List<ServiceStatsBindingModel> model;
            try
            {
                model = await _unitOfWork.ServiceRepository.GetAllWithStatsAsync(sort);
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

            return Ok(model);
        }

    }
}