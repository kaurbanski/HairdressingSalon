using Effort;
using Effort.DataLoaders;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity.Validation;
using System.IO;
using System.Linq;
using System.Linq.Dynamic;
using System.Text;
using System.Threading.Tasks;
using WebApplication3.Controllers;
using WebApplication3.Exceptions;
using WebApplication3.Models;
using WebApplication3.Repositories;

namespace WebApplication3.Tests.Controllers
{
    [TestClass]
    public class TestServiceRepository
    {
        private ApplicationDbContext _context;
        private IUnitOfWork unitOfWork;
        private IServiceRepository serviceRepo;


        [TestInitialize]
        public void SetUp()
        {
            var connection = DbConnectionFactory.CreateTransient();
            _context = new ApplicationDbContext(connection);
            serviceRepo = new ServiceRepository(_context);
            unitOfWork = new UnitOfWork(_context);
        }

        [TestMethod]
        public async Task GetAllAsync_ReturnAllServices()
        {
            // Arrange
            string sort = "name";
            _context.Services.AddRange(GetServicesForTest());
            await _context.SaveChangesAsync();

            // Act
            var result = await unitOfWork.ServiceRepository.GetAllAsync(sort);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(5, result.Count());
        }
        [TestMethod]
        [ExpectedException(typeof(ParseException))]
        public async Task GetAllAsync_ShouldThrowParseExceptionIfSortParamIncorrect()
        {
            // Arrange
            string sort = "incorrectFieldSort";
            _context.Services.AddRange(GetServicesForTest());
            await _context.SaveChangesAsync();

            // Act
            var result = await unitOfWork.ServiceRepository.GetAllAsync(sort);
        }

        [TestMethod]
        [ExpectedException(typeof(ItemNotFoundException))]
        public async Task GetAllAsync_ShouldReturnItemNotFoundExceptionIfResultNull()
        {
            // Arrange
            string sort = "name";

            // Act
            var result = await unitOfWork.ServiceRepository.GetAllAsync(sort);
        }

        [TestMethod]
        public async Task GetAllAsync_ShouldReturnOrderedByNameList()
        {
            // Arrange
            string sort = "name";
            var services = GetServicesForTest();
            _context.Services.AddRange(GetServicesForTest());
            await _context.SaveChangesAsync();
            var sortedList = services.OrderBy(x => x.Name).ToList();

            // Act
            var result = await unitOfWork.ServiceRepository.GetAllAsync(sort);

            // Assert
            CollectionAssert.AreEqual(result.ToList(), sortedList);
        }

        [TestMethod]
        public async Task Add_ShouldAddServiceToDb()
        {
            // Arrange
            Service service = GetServiceForTest();

            // Act
            var result = unitOfWork.ServiceRepository.Add(service);
            await unitOfWork.CompleteAsync();

            // Assert
            Assert.AreEqual(1, _context.Services.Count());
            Assert.IsNotNull(_context.Services.First());
            Assert.AreEqual(service, _context.Services.First());
        }

        [TestMethod]
        [ExpectedException(typeof(DbEntityValidationException))]
        public async Task Add_ShouldReturnDbEntityValidationExceptionIfNameIsEmpty()
        {
            // Arrange
            Service service = Service.Create("");

            // Act
            var result = unitOfWork.ServiceRepository.Add(service);
            await unitOfWork.CompleteAsync();
        }

        [TestMethod]
        public async Task Remove_ShouldDatabaseBeEmptyAfterRemove()
        {
            // Arrange
            Service service = GetServiceForTest();
            _context.Services.Add(service);
            await _context.SaveChangesAsync();

            // Act
            unitOfWork.ServiceRepository.Remove(service);
            await unitOfWork.CompleteAsync();

            // Assert
            Assert.AreEqual(0, _context.Services.Count());
        }
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task Remove_ShouldThrowInvalidOperationExceptionIfEntityNotFound()
        {
            // Arrange
            Service service = Service.Create(4, "example");

            // Act
            unitOfWork.ServiceRepository.Remove(service);
            await unitOfWork.CompleteAsync();
        }

        [TestMethod]
        public async Task FindAsync_ShouldReturnEntity()
        {
            // Arrange
            Service service = GetServiceForTest();
            _context.Services.Add(service);
            await _context.SaveChangesAsync();

            // Act
            var result = await unitOfWork.ServiceRepository.FindAsync(service.Id);

            // Assert 
            Assert.IsNotNull(result);
            Assert.AreEqual(service.Id, result.Id);
            Assert.AreEqual(service.Name, result.Name);
        }

        [TestMethod]
        [ExpectedException(typeof(ItemNotFoundException))]
        public async Task FindAsync_ShouldReturnItemNotFounExceptionIfResultEmpty()
        {
            // Arrange
            int id = 1;

            // Act
            var result = await unitOfWork.ServiceRepository.FindAsync(id);
        }

        [TestMethod]
        public async Task Update_ShouldUpdateEntity()
        {
            // Arrange
            Service service = Service.Create(1, "name");
            Service serviceForUpdate = Service.Create(service.Id, "updated");
            _context.Services.Add(service);
            await _context.SaveChangesAsync();

            // Act
            var result = await unitOfWork.ServiceRepository.UpdateAsync(serviceForUpdate);
            var a = await unitOfWork.CompleteAsync();

            // Assert
            var name = _context.Services.First().Name;
            Assert.AreEqual(serviceForUpdate, _context.Services.First());
        }

        [TestMethod]
        [ExpectedException(typeof(DbEntityValidationException))]
        public async Task Update_ShouldThrowValidationErrorIfEntityInvalidate()
        {
            // Assert 
            Service service = Service.Create("");

            // Act
            unitOfWork.ServiceRepository.Add(service);
            await unitOfWork.CompleteAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(ItemNotFoundException))]
        public async Task Update_ShouldThrowItemNotFoundExceptionIfEntityDoesntExist()
        {
            // Arrange
            Service service = GetServiceForTest();

            // Act
            var result = await unitOfWork.ServiceRepository.UpdateAsync(service);
            await unitOfWork.CompleteAsync();
        }

        [TestMethod]
        public async Task GetAllWithStatsAsync_ShouldReturnStatsForAllServices()
        {
            // Arrange
            string sort = "name";
            var visitTerm = GetVsisitTermForTest();
            _context.VisitTerms.Add(visitTerm);
            _context.Services.AddRange(GetServicesForTest());
            var user = Customer.Create("Kamil", "Urbanski", DateTime.Now.AddYears(-20), "500355022", "kamilur@wp.pl", 10);
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            var visits = PrepareVisitsForTest(user.Id);
            _context.Visits.AddRange(visits);
            await _context.SaveChangesAsync();

            // Act
            var result = await unitOfWork.ServiceRepository.GetAllWithStatsAsync(sort);

            // Assert
            Assert.IsNotNull(result);
            foreach (var r in result)
            {
                Assert.AreEqual(r.QuantityOfVisits, 1);
            }

        }

        [TestMethod]
        [ExpectedException(typeof(ItemNotFoundException))]
        public async Task GetAllWithStatsAsync_ShouldThrowItemNotFounExceptionIfItemsNotFound()
        {
            // Arrange
            string sort = "name";

            // Act
            var result = await unitOfWork.ServiceRepository.GetAllWithStatsAsync(sort);
        }

        [TestMethod]
        [ExpectedException(typeof(ParseException))]
        public async Task GetAllWithStatsAsync_ShouldThrowParseExceptionIfSortParamIncorrect()
        {
            // Arrange
            string sort = "incorrect param";
            _context.Services.AddRange(GetServicesForTest());
            await _context.SaveChangesAsync();

            // Act 
            var result = await unitOfWork.ServiceRepository.GetAllWithStatsAsync(sort);
        }


        private Service GetServiceForTest()
        {
            return Service.Create("service");
        }

        private IEnumerable<Service> GetServicesForTest()
        {
            return new List<Service>
            {
                Service.Create(1,"dsds"),
                Service.Create(2,"hjffgd"),
                Service.Create(3,"vcfdfd"),
                Service.Create(4,"kkuku"),
                Service.Create(5,"qwqwqwq"),
            };
        }

        private VisitTerm GetVsisitTermForTest()
        {

            return VisitTerm.Create(Constants.Constants.OPENING_HOUR_OF_SALON.AddDays(2));

        }

        private IEnumerable<Visit> PrepareVisitsForTest(string customerId)
        {
            List<Visit> visits = new List<Visit>();
            for (int i = 1; i <= GetServicesForTest().Count(); i++)
            {
                visits.Add(Visit.Create(i, 1, customerId, GetVsisitTermForTest().StartDate.AddMinutes(Constants.Constants.VISIT_TIME)));
            }
            return visits;
        }
    }
}
