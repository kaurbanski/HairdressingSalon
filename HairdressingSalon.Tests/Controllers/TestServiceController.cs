using AutoMapper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Results;
using WebApplication3.AutoMapper;
using WebApplication3.Constants;
using WebApplication3.Controllers;
using WebApplication3.Exceptions;
using WebApplication3.Models;
using WebApplication3.Models.ApiBindingModels;
using WebApplication3.Repositories;

namespace WebApplication3.Tests.Controllers
{
    [TestClass]
    public class TestServiceController
    {
        ServicesController controller;
        Mock<IUnitOfWork> unitOfWorkMock;
        Mock<IMapper> mapperMock;

        public TestServiceController()
        {
            unitOfWorkMock = new Mock<IUnitOfWork>();
            mapperMock = new Mock<IMapper>();
            controller = new ServicesController(unitOfWorkMock.Object, mapperMock.Object);
        }

        [TestMethod]
        public async Task AllServices_ReturnOkWithCorrectQuantityOfServices()
        {
            // Arrange
            string sort = "name";
            var services = GetServiceList();

            unitOfWorkMock.Setup(x => x.ServiceRepository.GetAllAsync(It.IsAny<string>())).ReturnsAsync(services);
            mapperMock.Setup(x => x.Map<IEnumerable<ServiceBindingModel>>(services)).Returns(GetServiceBindingModelList);

            // Act
            IHttpActionResult actionResult = await controller.Services(sort);
            var contentResult = actionResult as OkNegotiatedContentResult<IEnumerable<ServiceBindingModel>>;


            // Assert
            Assert.IsNotNull(contentResult, "Content result is null");
            Assert.IsNotNull(contentResult.Content, "Content in content result is null");
            Assert.AreEqual(contentResult.Content.Count(), services.Count());
        }

        [TestMethod]
        public async Task AllServices_ReturnNotFoundAfterItemNotFoundException()
        {
            // Arrange
            string sort = "name";
            unitOfWorkMock.Setup(x => x.ServiceRepository.GetAllAsync(It.IsAny<string>())).ThrowsAsync(new ItemNotFoundException());

            // Act
            IHttpActionResult actionResult = await controller.Services(sort);

            // Assert
            Assert.IsInstanceOfType(actionResult, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task AllServices_ReturnInternalServerErrorAfterError()
        {
            // Arrange
            string sort = "name";
            unitOfWorkMock.Setup(x => x.ServiceRepository.GetAllAsync(It.IsAny<string>())).ThrowsAsync(new DivideByZeroException());

            // Act
            IHttpActionResult actionResult = await controller.Services(sort);

            // Assert

            Assert.IsInstanceOfType(actionResult, typeof(InternalServerErrorResult));
        }

        [TestMethod]
        public async Task AllServices_InvalidModelStateResultAfterIncorrectSortParam()
        {
            // Arrange
            unitOfWorkMock.Setup(x => x.ServiceRepository.GetAllAsync(It.IsAny<string>())).ThrowsAsync(new ParseException("error", 1));

            // Act
            IHttpActionResult result = await controller.Services("incorrectSortParam");

            // Assert
            Assert.IsInstanceOfType(result, typeof(InvalidModelStateResult));
        }

        [TestMethod]
        public async Task Update_ReturnNotFoundAfterNotFoundException()
        {
            Service service = GetService();
            ServiceBindingModel model = GetModel();
            // Arrange
            unitOfWorkMock.Setup(x => x.ServiceRepository.UpdateAsync(It.IsAny<Service>())).ThrowsAsync(new ItemNotFoundException());

            // Act
            IHttpActionResult result = await controller.Update(model);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task Update_ReturnOkWithCorretcServiceId()
        {
            // Arrange
            Service service = GetService();
            ServiceBindingModel model = GetModel();
            unitOfWorkMock.Setup(x => x.ServiceRepository.UpdateAsync(service)).ReturnsAsync(service);
            unitOfWorkMock.Setup(x => x.CompleteAsync()).ReturnsAsync(1);

            // Acr
            IHttpActionResult result = await controller.Update(model);
            var contentResult = result as NegotiatedContentResult<ServiceBindingModel>;

            //Assert
            Assert.IsNotNull(contentResult);
            Assert.AreEqual(HttpStatusCode.Accepted, contentResult.StatusCode);
            Assert.IsNotNull(contentResult.Content);
            Assert.AreEqual(service.Id, contentResult.Content.Id);
        }

        [TestMethod]
        public async Task Update_ReturnInternalServerErrorAfterError()
        {

            // Arrange
            unitOfWorkMock.Setup(x => x.ServiceRepository.UpdateAsync(It.IsAny<Service>())).ThrowsAsync(new Exception());

            // Act
            IHttpActionResult result = await controller.Update(It.IsAny<ServiceBindingModel>());

            // Assert 
            Assert.IsInstanceOfType(result, typeof(InternalServerErrorResult));
        }

        [TestMethod]
        public async Task Add_ReturnInternalServerErrorAfterError()
        {
            // Arrange
            unitOfWorkMock.Setup(x => x.CompleteAsync()).ThrowsAsync(new Exception());

            // Act
            IHttpActionResult result = await controller.Add(It.IsAny<ServiceBindingModel>());

            // Assert
            Assert.IsInstanceOfType(result, typeof(InternalServerErrorResult));
        }

        [TestMethod]
        public async Task Add_ReturnOkResultWithAddedServiceAsContent()
        {
            Service service = GetService();
            ServiceBindingModel model = GetModel();
            // Arrange
            unitOfWorkMock.Setup(x => x.ServiceRepository.Add(service)).Returns(service);
            unitOfWorkMock.Setup(x => x.CompleteAsync()).ReturnsAsync(1);
            mapperMock.Setup(x => x.Map<ServiceBindingModel>(It.IsAny<Service>())).Returns(model);

            // Act
            IHttpActionResult result = await controller.Add(model);
            var contentResult = result as CreatedNegotiatedContentResult<ServiceBindingModel>;

            // Assert
            Assert.IsNotNull(contentResult);
            Assert.IsNotNull(contentResult.Content);
            Assert.AreEqual(contentResult.Content.Id, service.Id);
        }

        [TestMethod]
        public async Task Delete_ShouldReturnOkIfDeleteSuccess()
        {
            // Arrange
            Service service = GetService();
            unitOfWorkMock.Setup(x => x.ServiceRepository.FindAsync(service.Id)).ReturnsAsync(service);
            unitOfWorkMock.Setup(x => x.CompleteAsync()).ReturnsAsync(1);

            // Act
            IHttpActionResult result = await controller.Delete(service.Id);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkResult));
        }

        [TestMethod]
        public async Task Delete_ShouldReturnItemNotFounIfItemDoesntExist()
        {
            // Arrange
            Service service = GetService();
            unitOfWorkMock.Setup(x => x.ServiceRepository.RemoveById(service.Id)).ThrowsAsync(new ItemNotFoundException());

            // Act
            IHttpActionResult result = await controller.Delete(service.Id);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task Delete_ShouldReturnInternalServerErrorAfterThrowException()
        {
            // Arrange
            Service service = GetService();
            unitOfWorkMock.Setup(x => x.ServiceRepository.RemoveById(service.Id));

            // Act
            var result = await controller.Delete(service.Id);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(InternalServerErrorResult));
        }

        [TestMethod]
        public async Task GetAllWithStats_ShouldReturnOk()
        {
            // Arrange
            string sort = "name";
            unitOfWorkMock.Setup(x => x.ServiceRepository.GetAllWithStatsAsync(sort)).ReturnsAsync(GetServicesWithStats());

            // Act
            IHttpActionResult result = await controller.GetAllWithStats(sort);
            var contentResult = result as OkNegotiatedContentResult<List<ServiceStatsBindingModel>>;

            // Assert
            Assert.IsNotNull(contentResult);
            Assert.IsNotNull(contentResult.Content);
            Assert.AreEqual(contentResult.Content.Count(), GetServicesWithStats().Count);
        }

        [TestMethod]
        public async Task GetAllWithStats_ShouldRetunNotFoundAfterItemNotFoundResult()
        {
            // Arrange
            string sort = "name";
            unitOfWorkMock.Setup(x => x.ServiceRepository.GetAllWithStatsAsync(sort)).ThrowsAsync(new ItemNotFoundException());

            // Act
            IHttpActionResult result = await controller.GetAllWithStats(sort);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task GetAllWithStats_ShouldReturnInvalidModelStateAfterIncorrectSortParam()
        {
            // Arrange
            string sort = "incorrect param";
            unitOfWorkMock.Setup(x => x.ServiceRepository.GetAllWithStatsAsync(sort)).ThrowsAsync(new ParseException("error", 1));

            // Act
            IHttpActionResult result = await controller.GetAllWithStats(sort);
        }

        [TestMethod]
        public async Task GetAllWithStats_ShouldReturnInternalServerErrorAfterError()
        {
            // Arrange
            string sort = "name";
            unitOfWorkMock.Setup(x => x.ServiceRepository.GetAllWithStatsAsync(sort)).ThrowsAsync(new Exception());

            // Act
            IHttpActionResult result = await controller.GetAllWithStats(sort);

            //
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(InternalServerErrorResult));
        }


        public IEnumerable<Service> GetServiceList()
        {
            return new List<Service>
                {
                    Service.Create("usluga1"),
                    Service.Create("usluga2"),
                    Service.Create("usluga3")
                };
        }

        public IEnumerable<ServiceBindingModel> GetServiceBindingModelList()
        {
            List<ServiceBindingModel> model = new List<ServiceBindingModel>();
            IEnumerable<Service> services = GetServiceList();
            foreach (var s in services)
            {
                model.Add(new ServiceBindingModel { Id = s.Id, Name = s.Name });
            }
            return model;
        }

        public Service GetService()
        {
            return Service.Create(1, "testowa nazwa");
        }

        public ServiceBindingModel GetModel()
        {
            return new ServiceBindingModel { Id = 1, Name = "nowa nazwa" };
        }

        public List<ServiceStatsBindingModel> GetServicesWithStats()
        {
            return new List<ServiceStatsBindingModel>
            {
                new ServiceStatsBindingModel {Id = 1, Name = "test", QuantityOfVisits = 5},
                new ServiceStatsBindingModel {Id = 2, Name = "test2", QuantityOfVisits = 1 },
                new ServiceStatsBindingModel {Id = 3, Name = "test3", QuantityOfVisits = 2 }
            };
        }


    }
}
