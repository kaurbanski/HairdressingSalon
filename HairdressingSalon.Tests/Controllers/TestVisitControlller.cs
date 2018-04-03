using AutoMapper;
using Microsoft.AspNet.Identity;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Hosting;
using System.Web.Http.Results;
using System.Web.Http.Routing;
using WebApplication3.Controllers;
using WebApplication3.Exceptions;
using WebApplication3.Models;
using WebApplication3.Models.ApiBindingModels;
using WebApplication3.Repositories;
using WebApplication3.Results;
using WebApplication3.Tests.DateTimeUtils;

namespace WebApplication3.Tests.Controllers
{
    [TestClass]
    public class TestVisitControlller
    {
        VisitController controller;
        Mock<IUnitOfWork> unitOfWorkMock;
        Mock<IMapper> mapperMock;

        public TestVisitControlller()
        {
            unitOfWorkMock = new Mock<IUnitOfWork>();
            mapperMock = new Mock<IMapper>();
            controller = new VisitController(unitOfWorkMock.Object, mapperMock.Object);
        }
        [TestMethod]
        public async Task Add_ShouldReturnOkNegotiatedContentResult()
        {

            // Arrange
            DateTime date = DateTime.Now.AddDays(1);

            unitOfWorkMock.Setup(x => x.VisitTermRepository.AddVisitTermOrReturnIfExists(It.IsAny<DateTime>())).ReturnsAsync(GetVisitTerm());
            unitOfWorkMock.Setup(x => x.VisitRepository.Add(It.IsAny<Visit>())).ReturnsAsync(GetVisit());
            unitOfWorkMock.Setup(x => x.VisitRepository.FindWithCustomerAndServiceAsync(It.IsAny<int>())).ReturnsAsync(GetVisit());
            mapperMock.Setup(x => x.Map<VisitInfoBindingModel>(It.IsAny<Visit>())).Returns(GetVisitInfoBindingModel());
            mapperMock.Setup(x => x.Map<AddVisitBindingModel, Visit>(It.IsAny<AddVisitBindingModel>(), It.IsAny<Visit>())).Returns(GetVisit());

            // Act
            IHttpActionResult result = await controller.Add(GetVisitBindingModel());
            var contentResult = result as OkNegotiatedContentResult<VisitInfoBindingModel>;

            // Assert
            Assert.IsNotNull(contentResult);
            Assert.IsNotNull(contentResult.Content);
        }

        [TestMethod]
        public async Task Add_ShouldReturnInvalidModelStateResultAfterVisitCurrentlyReservedException()
        {
            // Arrange
            unitOfWorkMock.Setup(x => x.VisitTermRepository.AddVisitTermOrReturnIfExists(It.IsAny<DateTime>())).ReturnsAsync(GetVisitTerm());
            unitOfWorkMock.Setup(x => x.VisitRepository.Add(It.IsAny<Visit>())).ThrowsAsync(new VisitCurrentlyReservedException());

            // Act
            IHttpActionResult result = await controller.Add(GetVisitBindingModel());

            // Assert
            Assert.IsInstanceOfType(result, typeof(InvalidModelStateResult));
        }

        [TestMethod]
        public async Task Add_ShouldReturnInternalServerErrorAfterException()
        {
            // Arrange
            unitOfWorkMock.Setup(x => x.VisitTermRepository.IsInDatabase(It.IsAny<DateTime>())).ReturnsAsync(true);
            unitOfWorkMock.Setup(x => x.VisitTermRepository.GetByDate(It.IsAny<DateTime>())).ReturnsAsync(GetVisitTerm());
            unitOfWorkMock.Setup(x => x.VisitRepository.Add(It.IsAny<Visit>())).ThrowsAsync(new Exception());

            // Act
            IHttpActionResult result = await controller.Add(GetVisitBindingModel());

            // Assert
            Assert.IsInstanceOfType(result, typeof(InternalServerErrorResult));
        }

        [TestMethod]
        public async Task Delete_ShouldReturnItemNotFoundResultIfItemNotFound()
        {
            // Arrange
            int id = 1;
            unitOfWorkMock.Setup(x => x.VisitRepository.FindAsync(It.IsAny<int>())).ThrowsAsync(new ItemNotFoundException());

            // Act
            IHttpActionResult result = await controller.Delete(id);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task Delete_ShouldReturnInternalServerErrorIfError()
        {
            // Arrange
            int id = 1;
            unitOfWorkMock.Setup(x => x.VisitRepository.FindAsync(It.IsAny<int>())).ThrowsAsync(new Exception());

            // Act
            IHttpActionResult result = await controller.Delete(id);

            // Assert
            Assert.IsInstanceOfType(result, typeof(InternalServerErrorResult));
        }

        [TestMethod]
        public async Task Delete_ShouldReturnOkIfUserIsInRoleAdmin()
        {
            // Arrange
            int id = 1;
            unitOfWorkMock.Setup(x => x.VisitRepository.FindAsync(It.IsAny<int>())).ReturnsAsync(GetVisit());
            PrepareAdminUserForController();

            // Act
            IHttpActionResult result = await controller.Delete(id);

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkResult));
        }

        [TestMethod]
        public async Task Delete_ShouldReturnOkIfUserIsVisitCustomer()
        {
            // Arrange
            int id = 1;
            unitOfWorkMock.Setup(x => x.VisitRepository.FindAsync(It.IsAny<int>())).ReturnsAsync(GetVisit());

            var claim = new Claim("test", GetCustomer().Id);
            var mockIdentity = Mock.Of<ClaimsIdentity>(x => x.FindFirst(It.IsAny<string>()) == claim);

            var userMock = new Mock<IPrincipal>();

            userMock.Setup(x => x.IsInRole("Admin")).Returns(false);
            userMock.Setup(x => x.Identity).Returns(mockIdentity);

            controller.User = userMock.Object;

            // Act
            IHttpActionResult result = await controller.Delete(id);

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkResult));
        }

        [TestMethod]
        public async Task Delete_ShouldReturnForbiddenActionResultIsUserIsntInRoleAdminAndCustomerIsntVisitCustomer()
        {
            // Arrange
            int id = 1;
            unitOfWorkMock.Setup(x => x.VisitRepository.FindAsync(It.IsAny<int>())).ReturnsAsync(GetVisit());

            var claim = new Claim("test", "other_customer_id");     // Claim with user password
            var mockIdentity = Mock.Of<ClaimsIdentity>(x => x.FindFirst(It.IsAny<string>()) == claim);

            var userMock = new Mock<IPrincipal>();

            userMock.Setup(x => x.IsInRole("Admin")).Returns(false);
            userMock.Setup(x => x.Identity).Returns(mockIdentity);

            controller.User = userMock.Object;

            // Act
            IHttpActionResult result = await controller.Delete(id);

            // Assert
            Assert.IsInstanceOfType(result, typeof(ForbiddenActionResult));
        }
        [TestMethod]
        public async Task Update_ShouldReturnItemNotFoundResultIfItemNotFound()
        {
            // Arrange
            UpdateVisitBindingModel model = new UpdateVisitBindingModel { Cost = 100, Id = 99999, ServiceId = 1 };
            unitOfWorkMock.Setup(x => x.VisitRepository.FindWithCustomerAndServiceAsync(It.IsAny<int>())).ThrowsAsync(new ItemNotFoundException());

            // Act
            IHttpActionResult result = await controller.Update(model);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task Update_ShouldReturnInternalServerErrorIfExcepion()
        {
            // Arrange
            UpdateVisitBindingModel model = new UpdateVisitBindingModel { Cost = 100, Id = 99999, ServiceId = 1 };
            unitOfWorkMock.Setup(x => x.VisitRepository.FindWithCustomerAndServiceAsync(model.Id)).ThrowsAsync(new Exception());

            // Act
            IHttpActionResult result = await controller.Update(model);

            // Assert
            Assert.IsInstanceOfType(result, typeof(InternalServerErrorResult));
        }

        [TestMethod]
        public async Task Visits_ShouldReturnNotFoundResult()
        {
            // Arrange
            unitOfWorkMock.Setup(x => x.VisitTermRepository.GetByDateWithVisits(It.IsAny<DateTime>())).ThrowsAsync(new ItemNotFoundException());

            // Act
            IHttpActionResult result = await controller.Visits(DateTime.Now);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task Visits_ShouldReturnInternalServerErrror()
        {
            // Arrange
            unitOfWorkMock.Setup(x => x.VisitTermRepository.GetByDateWithVisits(It.IsAny<DateTime>())).ThrowsAsync(new Exception());

            // Act
            IHttpActionResult result = await controller.Visits(DateTime.Now);

            // Assert
            Assert.IsInstanceOfType(result, typeof(InternalServerErrorResult));
        }

        [TestMethod]
        public async Task Visits_ShouldReturnOkWithVisits()
        {
            // Arrange
            VisitTerm visitTerm = GetVisitTerm();
            unitOfWorkMock.Setup(x => x.VisitTermRepository.GetByDateWithVisits(It.IsAny<DateTime>())).ReturnsAsync(visitTerm);
            mapperMock.Setup(x => x.Map<VisitTermInfoBindingModel>(It.IsAny<VisitTerm>())).Returns(new VisitTermInfoBindingModel());

            // Act
            IHttpActionResult result = await controller.Visits(DateTime.Now);
            var content = result as OkNegotiatedContentResult<VisitTermInfoBindingModel>;

            // Assert
            Assert.IsInstanceOfType(content, typeof(OkNegotiatedContentResult<VisitTermInfoBindingModel>));
            Assert.IsNotNull(content.Content);
        }

        [TestMethod]
        public async Task FreeHours_ShouldReturnInternalServerError()
        {
            // Arrange
            unitOfWorkMock.Setup(x => x.VisitTermRepository.IsInDatabase(It.IsAny<DateTime>())).ThrowsAsync(new Exception());

            // Act
            IHttpActionResult result = await controller.FreeHours(new FreeHoursBindingModel());

            // Assert
            Assert.IsInstanceOfType(result, typeof(InternalServerErrorResult));
        }

        [TestMethod]
        public async Task FreeHours_ShouldReturnOkNegotiatedContentResult()
        {
            // Arrange
            unitOfWorkMock.Setup(x => x.VisitTermRepository.IsInDatabase(It.IsAny<DateTime>())).ReturnsAsync(true);
            unitOfWorkMock.Setup(x => x.VisitTermRepository.GetFreeHours(It.IsAny<DateTime>())).ReturnsAsync(GetVisitsTermByDateBindingModel());

            // Act
            IHttpActionResult result = await controller.FreeHours(new FreeHoursBindingModel());
            var content = result as OkNegotiatedContentResult<List<GetVisitTermByDateBindingModel>>;

            // Assert
            Assert.IsNotNull(content);
            Assert.IsNotNull(content.Content);
            Assert.AreEqual(GetVisitsTermByDateBindingModel().Count, content.Content.Count());
        }

        [TestMethod]
        public async Task FreeHours_ShouldReturnNotFoundResult()
        {
            // Arrange
            unitOfWorkMock.Setup(x => x.VisitTermRepository.IsInDatabase(It.IsAny<DateTime>())).ReturnsAsync(true);
            unitOfWorkMock.Setup(x => x.VisitTermRepository.GetFreeHours(It.IsAny<DateTime>())).ThrowsAsync(new ItemNotFoundException());

            // Act
            IHttpActionResult result = await controller.FreeHours(new FreeHoursBindingModel());

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task UserVisits_ShouldRetrunForbiddenActionResultIfUserIsOtherThanOwnerOfVisits()
        {
            // Arrange
            var claim = new Claim("test", "other_customer_id");     // Claim with user password
            var mockIdentity = Mock.Of<ClaimsIdentity>(x => x.FindFirst(It.IsAny<string>()) == claim);

            var userMock = new Mock<IPrincipal>();
            userMock.Setup(x => x.IsInRole("Admin")).Returns(false);
            userMock.Setup(x => x.Identity).Returns(mockIdentity);

            controller.User = userMock.Object;

            // Act
            IHttpActionResult result = await controller.UserVisits("example@gmail.com");

            // Assert
            Assert.IsInstanceOfType(result, typeof(ForbiddenActionResult));
        }

        [TestMethod]
        public async Task UserVisits_ShouldReturnNotFoundResultIfItemNotFound()
        {
            // Arrange
            unitOfWorkMock.Setup(x => x.VisitRepository.VisitsCountAsync(It.IsAny<string>())).ThrowsAsync(new ItemNotFoundException());
            PrepareAdminUserForController();

            // Act
            IHttpActionResult result = await controller.UserVisits("email@gmail.com");

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task UserVisits_ShouldReturnInternalServerErrorIfException()
        {
            // Arrange
            PrepareAdminUserForController();
            unitOfWorkMock.Setup(x => x.VisitRepository.VisitsCountAsync(It.IsAny<string>())).ThrowsAsync(new Exception());

            // Act
            IHttpActionResult result = await controller.UserVisits("email");

            // Assert
            Assert.IsInstanceOfType(result, typeof(InternalServerErrorResult));
        }

        [TestMethod]
        public async Task UserVisits_ShouldReturnInvalidModelStateResultIfSortParamIncorrect()
        {
            // Arrange
            PrepareAdminUserForController();
            unitOfWorkMock.Setup(x => x.VisitRepository.VisitsCountAsync(It.IsAny<string>())).ReturnsAsync(10);
            unitOfWorkMock.Setup(x => x.VisitRepository.GetByUserAsync(It.IsAny<string>(), 1, 1, It.IsAny<string>())).ThrowsAsync(new ParseException("error", 1));

            // Act
            IHttpActionResult result = await controller.UserVisits("email", 1, 1, "param");

            // Assert
            Assert.IsInstanceOfType(result, typeof(InvalidModelStateResult));
        }

        //TODO skończy to
        //[TestMethod]
        //public async Task UserVisits_ShouldReturnOkResultWithContent()
        //{
        //    // Arrange
        //    PrepareAdminUserForController();
        //    unitOfWorkMock.Setup(x => x.VisitRepository.VisitsCountAsync(It.IsAny<string>())).ReturnsAsync(10);
        //    unitOfWorkMock.Setup(x => x.VisitRepository.GetByUserAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>())).ReturnsAsync(GetVisits());
        //    mapperMock.Setup(x => x.Map<List<UserVisitInfoBindingModel>>(It.IsAny<List<Visit>>())).Returns(new List<UserVisitInfoBindingModel>());
        //    string expectedUrl = "http://fakeurl.com";

        //    var mockUrlHelper = new Mock<UrlHelper>();
        //    mockUrlHelper.Setup(x => x.Link(It.IsAny<string>(), It.IsAny<object>())).Returns("test_url");

        //    controller.Url = mockUrlHelper.Object;

        //    //controller.Request = new HttpRequestMessage(HttpMethod.Get, expectedUrl);
        //    //UrlHelper helper = new UrlHelper(controller.Request);
        //    //var mockUrlHelper = new Mock<UrlHelper>();
        //    //controller.Request = new System.Net.Http.HttpRequestMessage(HttpMethod.Get, expectedUrl);
        //    // mockUrlHelper.Setup(x => x.Link).
        //    //controller.Url = 

        //    // Act
        //    IHttpActionResult result = await controller.UserVisits("email@gmail.com");

        //    // Assert
        //    Assert.IsInstanceOfType(result, typeof(OkNegotiatedContentResult<List<UserVisitInfoBindingModel>>));
        //}

        public void PrepareAdminUserForController()
        {
            var userMock = new Mock<IPrincipal>();
            userMock.Setup(x => x.IsInRole("Admin")).Returns(true);
            userMock.SetupGet(x => x.Identity.Name).Returns(GetCustomer().UserName);
            userMock.SetupGet(x => x.Identity.IsAuthenticated).Returns(true);

            var requestContextMock = new Mock<HttpRequestContext>();
            requestContextMock.Setup(x => x.Principal).Returns(userMock.Object);
            controller.RequestContext = requestContextMock.Object;
            controller.Request = new System.Net.Http.HttpRequestMessage();
            controller.Configuration = new HttpConfiguration();
        }

        public VisitTerm GetVisitTerm()
        {
            return VisitTerm.Create(
                DateTimeUtil.GetNextWeekday(Constants.Constants.OPENING_HOUR_OF_SALON, DayOfWeek.Monday),
                1);
        }

        public List<Visit> GetVisits()
        {
            return new List<Visit>()
            {
                Visit.Create(1,1,1,"custo_id", DateTime.Now),
                Visit.Create(2,2,2,"custo2_id", DateTime.Now.AddYears(1)),
                Visit.Create(3,3,3,"custo3_id",DateTime.Now.AddYears(-2))
            };
        }

        public Visit GetVisit()
        {
            return Visit.Create(1, GetService().Id, GetCustomer().Id, GetVisitTerm().StartDate);
        }

        public List<GetVisitTermByDateBindingModel> GetVisitsTermByDateBindingModel()
        {
            return new List<GetVisitTermByDateBindingModel>(){
                new GetVisitTermByDateBindingModel() {Date = DateTime.Now },
                new GetVisitTermByDateBindingModel() {Date = DateTime.Now.AddHours(1) }
            };
        }

        public Customer GetCustomer()
        {
            return Customer.Create("id_of_customer", "name", "lastname", DateTime.Now.AddYears(-20), "4343 43533", "email@gmail.com", 0);
        }

        public Service GetService()
        {
            return Service.Create(1, "service test");
        }

        public AddVisitBindingModel GetVisitBindingModel()
        {
            return new AddVisitBindingModel { ServiceId = GetService().Id, StartDate = GetVisitTerm().StartDate };
        }

        public VisitInfoBindingModel GetVisitInfoBindingModel()
        {
            return new VisitInfoBindingModel { Id = GetVisit().Id, StartDate = GetVisit().StartDate };
        }
    }
}
