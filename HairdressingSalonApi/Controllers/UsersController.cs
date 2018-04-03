using AutoMapper;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using NLog;
using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Linq;
using System.Linq.Dynamic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using WebApplication3.Exceptions;
using WebApplication3.Helpers;
using WebApplication3.Models;
using WebApplication3.Models.ApiViewModels;
using WebApplication3.Repositories;
using WebApplication3.RoleManager;

namespace WebApplication3.Controllers
{
    [RoutePrefix("api/users")]
    public class UsersController : ApiController
    {
        private readonly IUnitOfWork _unitOfWork;
        private static NLog.Logger _logger;
        private IMapper _mapper;
        private ApplicationUserManager _userManager;

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? Request.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        public UsersController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _logger = LogManager.GetCurrentClassLogger();
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        //protected ApplicationRoleManager AppRoleManager
        //{
        //    get
        //    {
        //        return _appRoleManager ?? Request.GetOwinContext().GetUserManager<ApplicationRoleManager>();
        //    }
        //}

        [Route("register")]
        [AllowAnonymous]
        public async Task<IHttpActionResult> Register(RegisterBindingModel model)
        {

            var user = _mapper.Map<ApplicationUser>(model);
            user.UserName = user.Email;
            
            var userManager = Request.GetOwinContext().GetUserManager<ApplicationUserManager>();

            
            IdentityResult result = await userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
            {
                return GetErrorResult(result);
            }

            await userManager.AddToRoleAsync(user.Id, "Customer");

            return Ok(model);
        }

        [HttpGet]
        [Route("customers", Name = "CustomersRoute")]
        [Authorize(Roles = "Admin")]
        public async Task<IHttpActionResult> Customers(string sort = "lastname", int pageNo = 1, int pageSize = 5)
        {
            int customersCount;
            int maximumPageSize = 50;
            pageSize = pageSize > maximumPageSize ? maximumPageSize : pageSize;

            List<CustomerInfoBindingModel> result;
            PageLinkBuilder linkBuilder = null;
            try
            {
                var customers = await _unitOfWork.UserRepository.GetCustomersAsync(sort, pageNo, pageSize);
                result = _mapper.Map<List<Customer>, List<CustomerInfoBindingModel>>(customers);
                var userManager = System.Web.HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();
                customersCount = await _unitOfWork.UserRepository.GetAllCountAsync("customer");
                linkBuilder = new PageLinkBuilder(Url, "CustomersRoute", new { sort = sort }, pageNo, pageSize, customersCount);
            }
            catch (ItemNotFoundException e)
            {
                return NotFound();
            }
            catch (ParseException e)
            {
                ModelState.AddModelError("sort", "Incorrect parameter!");
                return BadRequest(ModelState);
            }
            catch (Exception e)
            {
                _logger.Error(e);
                return InternalServerError();
            }
            HttpContext.Current.Response.Headers.Add("X-Pagination",
                Newtonsoft.Json.JsonConvert.SerializeObject(linkBuilder));
            return Ok(result);
        }

        [HttpGet]
        [Route("customers-with-stats", Name = "CustomersWithStatsRoute")]
        [Authorize(Roles = "Admin")]
        public async Task<IHttpActionResult> CustomersWithStats(string sort = "customer.lastname", int pageNo = 1, int pageSize = 5)
        {
            int customersCount;
            int maximumPageSize = 50;
            pageSize = pageSize > maximumPageSize ? maximumPageSize : pageSize;

            List<CustomerStatsApiModel> result;
            PageLinkBuilder linkBuilder = null;
            try
            {
                result = await _unitOfWork.UserRepository.GetCustomersWithStats(sort, pageNo, pageSize);
                customersCount = await _unitOfWork.UserRepository.GetAllCountAsync("customer");
                linkBuilder = new PageLinkBuilder(Url, "CustomersWithStatsRoute", new { sort = sort }, pageNo, pageSize, customersCount);
            }
            catch (ItemNotFoundException e)
            {
                return NotFound();
            }
            catch (ParseException e)
            {
                ModelState.AddModelError("sort", "Incorrect parameter!");
                return BadRequest(ModelState);
            }
            catch (Exception e)
            {
                _logger.Error(e);
                return InternalServerError();
            }
            HttpContext.Current.Response.Headers.Add("X-Pagination",
                Newtonsoft.Json.JsonConvert.SerializeObject(linkBuilder));
            return Ok(result);
        }

        [Route("")]
        [HttpDelete]
        [Authorize(Roles = "Admin")]
        public async Task<IHttpActionResult> Delete(string userName)
        {
            IdentityResult result = new IdentityResult();

            if (string.IsNullOrWhiteSpace(userName))
            {
                return NotFound();
            }
            try
            {
                var userManager = System.Web.HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();
                var user = await userManager.FindByNameAsync(userName);
                if (user == null)
                {
                    return NotFound();
                }
                result = await userManager.DeleteAsync(user);
            }
            catch (Exception e)
            {
                _logger.Error(e);
                return InternalServerError();
            }
            if (result.Succeeded)
            {
                return Ok();
            }

            return NotFound();
        }
        [Route("add/customer")]
        [HttpPost]
        [Authorize(Roles = "Admin, Hairdresser")]
        public async Task<IHttpActionResult> AddCustomer(UserAddBindingModel user)
        {
            IdentityResult roleResult = new IdentityResult();
            IdentityResult userResult = new IdentityResult();
            Customer mapedUser;

            try
            {
                mapedUser = _mapper.Map<Customer>(user);
                mapedUser.UserName = user.Email;
                var userManager = System.Web.HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();
                userResult = await userManager.CreateAsync(mapedUser, user.Password);

                if (!userResult.Succeeded)
                {
                    return BadRequest();
                }

                roleResult = await userManager.AddToRoleAsync(mapedUser.Id, "Customer");
            }
            catch (Exception e)
            {
                _logger.Error(e);
                return InternalServerError();
            }

            if (!roleResult.Succeeded)
            {
                return InternalServerError();
            }
            else
            {
                var resultUser = _mapper.Map<UserInfoBindingModel>(user);
                return Created("Created successfully", resultUser);
            }
        }


        [Route("")]
        [HttpPut]
        [Authorize]
        public async Task<IHttpActionResult> Update(UserInfoBindingModel model)
        {
            if (!User.IsInRole("Admin") && !(User.Identity.GetUserName() == model.Email))
            {
                return StatusCode(HttpStatusCode.Forbidden);
            }

            IdentityResult result = new IdentityResult();

            var userManager = System.Web.HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();
            var user = await userManager.FindByEmailAsync(model.Email);

            if (user == null)
            {
                return NotFound();
            }

            try
            {
                var mapedUser = _mapper.Map(model, user);
                result = await userManager.UpdateAsync(mapedUser);
            }
            catch (Exception e)
            {
                _logger.Error(e);
                return InternalServerError();
            }

            if (!result.Succeeded)
            {
                return InternalServerError();
            }
            else
            {
                return Ok(model);
            }
        }

        [HttpPut]
        [Route("set-customer-discount")]
        [Authorize(Roles = "Admin, Hairdresser")]
        public async Task<IHttpActionResult> SetCustomerDiscount(CustomerSetDiscountModel model)
        {
            Customer user = null;
            try
            {
                var userManager = System.Web.HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();
                user = (Customer)await userManager.FindByEmailAsync(model.Email);
                user.SetDiscount(model.PercentDiscount);
                await userManager.UpdateAsync(user);
            }
            catch (Exception e)
            {
                _logger.Error(e);
                return InternalServerError();
            }
            var result = _mapper.Map<CustomerInfoBindingModel>(user);
            return Ok(result);
        }

        [HttpPost]
        [Route("change-password")]
        [Authorize]
        public async Task<IHttpActionResult> ChangePassword(ChangePasswordBindingModel model)
        {
            IdentityResult result = new IdentityResult();

            var userManager = Request.GetOwinContext().GetUserManager<ApplicationUserManager>();
            var userId = RequestContext.Principal.Identity.GetUserId();

            try
            {
                result = await userManager.ChangePasswordAsync(userId, model.OldPassword, model.NewPassword);
            }
            catch (Exception e)
            {
                _logger.Error(e);
                return InternalServerError();
            }

            if (!result.Succeeded)
            {
                return InternalServerError();
            }
            else
            {
                return Ok();
            }
        }

        [Route("hairdressers", Name = "HairddressersRoute")]
        [HttpGet]
        [Authorize(Roles = "Admin, Hairdresser")]
        public async Task<IHttpActionResult> Hairdressers(string sort = "lastname", int pageNo = 1, int pageSize = 5)
        {
            List<UserInfoBindingModel> result;
            int maximumPageSize = 50;
            pageSize = pageSize > maximumPageSize ? maximumPageSize : pageSize;
            int count;
            PageLinkBuilder linkBuilder;

            try
            {
                var hairdressers = await _unitOfWork.UserRepository.GetAllHairdressersAsync(sort, pageNo, pageSize);
                result = _mapper.Map<List<ApplicationUser>, List<UserInfoBindingModel>>(hairdressers);
                count = await _unitOfWork.UserRepository.GetAllCountAsync("Hairdresser");
                linkBuilder = new PageLinkBuilder(Url, "HairddressersRoute", new { sort = sort }, pageNo, pageSize, count);
            }
            catch (ItemNotFoundException e)
            {
                return NotFound();
            }
            catch (ParseException e)
            {
                ModelState.AddModelError("sort", "Incorrect parameter!");
                return BadRequest(ModelState);
            }
            catch (Exception e)
            {
                _logger.Error(e);
                return InternalServerError();
            }

            HttpContext.Current.Response.Headers.Add("X-Pagination",
                Newtonsoft.Json.JsonConvert.SerializeObject(linkBuilder));

            return Ok(result);
        }

        [Route("add/hairdresser")]
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IHttpActionResult> AddHairdresser(UserAddBindingModel user)
        {
            IdentityResult roleResult = new IdentityResult();
            IdentityResult userResult = new IdentityResult();

            try
            {
                var mapedUser = _mapper.Map<ApplicationUser>(user);
                mapedUser.UserName = user.Email;
                var userManager = System.Web.HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();
                userResult = await userManager.CreateAsync(mapedUser, user.Password);

                if (!userResult.Succeeded)
                {
                    return InternalServerError();
                }

                roleResult = await userManager.AddToRoleAsync(mapedUser.Id, "Hairdresser");
            }
            catch (Exception e)
            {
                _logger.Error(e);
                return InternalServerError();
            }
            if (!roleResult.Succeeded)
            {
                return InternalServerError();
            }
            else
            {
                return Ok();
            }
        }

        [HttpGet]
        [Route("user-details/{email}")]
        [Authorize]
        public async Task<IHttpActionResult> GetUserDetails(string email)
        {
            Customer user;
            CustomerInfoBindingModel model;
            try
            {
                user = await _unitOfWork.UserRepository.FindCustomerByEmailAsync(email);
                model = _mapper.Map<CustomerInfoBindingModel>(user);
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
            return Ok(model);
        }

        [HttpGet]
        [Route("customer-stats/{email}")]
        [Authorize(Roles = "Admin, Hairdresser")]
        public async Task<IHttpActionResult> GetCustomerStats(string email)
        {
            CustomerFullStatsModel model = new CustomerFullStatsModel();
            try
            {
                model = await _unitOfWork.UserRepository.GetCustomerStats(email);
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
            var result = _mapper.Map<CustomerStatsApiModel>(model);
            return Ok(result);
        }

        [HttpGet]
        [Route("search", Name = "SearchUsersRoute")]
        [Authorize(Roles = "Admin, Hairdresser")]
        public async Task<IHttpActionResult> GetUsersByPhrase(string phrase, int pageNo = 1, int pageSize = 5, string sort = "lastname", string role = "Customer")
        {
            List<ApplicationUser> users;
            PageLinkBuilder linkBuilder;

            try
            {
                var usersCount = await _unitOfWork.UserRepository.GetByPhraseAndRoleCountAsync(phrase, role);
                users = await _unitOfWork.UserRepository.GetUserByPhraseAndRole(sort, pageNo, pageSize, phrase, role);
                linkBuilder = new PageLinkBuilder(Url, "SearchUsersRoute", new { sort = sort, role = role, phrase = phrase }, pageNo, pageSize, usersCount);
            }
            catch (ItemNotFoundException e)
            {
                return NotFound();
            }
            catch (ParseException e)
            {
                ModelState.AddModelError("sort", "Incorrect parameter!");
                return BadRequest(ModelState);
            }
            catch (Exception e)
            {
                _logger.Error(e);
                return InternalServerError();
            }
            var result = _mapper.Map<List<UserInfoBindingModel>>(users);
            HttpContext.Current.Response.Headers.Add("x-pagination", Newtonsoft.Json.JsonConvert.SerializeObject(linkBuilder));

            return Ok(result);
        }

        private IHttpActionResult GetErrorResult(IdentityResult result)
        {
            if (result == null)
            {
                return InternalServerError();
            }

            if (!result.Succeeded)
            {
                if (result.Errors != null)
                {
                    foreach (string error in result.Errors)
                    {
                        ModelState.AddModelError("", error);
                    }
                }

                if (ModelState.IsValid)
                {
                    // No ModelState errors are available to send, so just return an empty BadRequest.
                    return BadRequest();
                }

                return BadRequest(ModelState);
            }

            return null;
        }

        //[HttpPut]
        //[Route("block")]
        //[Authorize(Roles = "Admin")]
        //public async Task<IHttpActionResult> BlockUnblockUser(string email)
        //{
        //    ApplicationUser user;
        //    try
        //    {
        //        var userManager = System.Web.HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();
        //        user = await userManager.FindByEmailAsync(email);
        //        var isBlocked = await userManager.GetLockoutEnabledAsync(user.Id);

        //        if (isBlocked)
        //        {
        //            await userManager.SetLockoutEnabledAsync(user.Id, false);
        //            await userManager.SetLockoutEndDateAsync(user.Id, DateTimeOffset.MinValue);
        //        }
        //        else
        //        {
        //            await userManager.SetLockoutEnabledAsync(user.Id, true);
        //            await userManager.SetLockoutEndDateAsync(user.Id, DateTimeOffset.MaxValue);
        //        }
        //    }
        //    catch (ItemNotFoundException e)
        //    {
        //        return NotFound();
        //    }
        //    catch (Exception e)
        //    {
        //        _logger.Error(e, "Failed in:" + Request.RequestUri);
        //        return InternalServerError();
        //    }

        //    var result = Mapper.Map<BlockUserModel>(user);
        //    return Ok(result);
        //}
    }
}