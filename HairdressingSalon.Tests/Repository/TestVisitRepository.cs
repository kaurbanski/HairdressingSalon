using Effort;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebApplication3.Models;
using System.Data.Entity;
using WebApplication3.Repositories;
using WebApplication3.Exceptions;
using Moq;
using System.Data.Entity.Validation;
using System.Linq.Dynamic;
using WebApplication3.Tests.DateTimeUtils;

namespace WebApplication3.Tests.Repository
{
    [TestClass]
    public class TestVisitRepository
    {
        private ApplicationDbContext _context;
        private IUnitOfWork _unitOfWork;
        private IVisitRepository _visitRepository;

        [TestInitialize]
        public void SetUp()
        {
            var connection = DbConnectionFactory.CreateTransient();
            _context = new ApplicationDbContext(connection);
            _visitRepository = new VisitRepository(_context);
            _unitOfWork = new UnitOfWork(_context);
            PrepareData();
        }

        [TestMethod]
        public async Task IsReserved_ShouldReturnTrueIfTermReserved()
        {
            // Arrange 
            var reservedVisit = _context.Visits.First();

            // Act
            var reserved = await _unitOfWork.VisitRepository.IsReserved(reservedVisit.StartDate);

            // Assert
            Assert.IsTrue(reserved);
        }

        [TestMethod]
        public async Task IsReserved_ShouldReturnFalseIfTermNotReserved()
        {
            // Arrange 
            Visit visit = Visit.Create(1, 1, "custId", DateTime.Now);

            // Act
            var reserved = await _unitOfWork.VisitRepository.IsReserved(visit.StartDate);

            // Assert
            Assert.IsFalse(reserved);
        }

        [TestMethod]
        public async Task Update_ShouldChangeCostAndCostAfterDiscountForVisit()
        {
            // Arrange
            decimal cost = 105;
            Visit visit = _context.Visits.Include(x => x.Customer).First();
            Customer customer = visit.Customer;
            Service service = _context.Services.First();
            visit.SetCost(cost);
            visit.SetServiceId(service.Id);

            // Act
            Visit result = await _unitOfWork.VisitRepository.Update(visit);
            await _unitOfWork.CompleteAsync();
            Visit updatedVisit = _context.Visits.Where(x => x.Id == visit.Id).First();

            // Assert
            Assert.AreEqual(updatedVisit.Cost, cost);
            Assert.AreEqual(updatedVisit.CostAfterDiscount, Constants.CostUtils.GetCostOfTheVisitAfterDiscount(cost, customer.PercentDiscount));
            Assert.AreEqual(service.Id, updatedVisit.ServiceId);
        }

        [TestMethod]
        [ExpectedException(typeof(ItemNotFoundException))]
        public async Task Update_ShouldThrowItemNotFoundExceptionIfVisitDoesntExist()
        {
            // Arrange
            Visit visit = await _context.Visits.FirstAsync();
            _context.Visits.Remove(visit);
            await _context.SaveChangesAsync();


            // Act
            Visit result = await _unitOfWork.VisitRepository.Update(visit);
        }

        [TestMethod]
        [ExpectedException(typeof(DbEntityValidationException))]
        public async Task Update_ShouldThrowDbEntityValidationExceptionIfCostOutOfRange()
        {
            // Assert
            decimal incorrectCost = -100;
            Visit visit = await _context.Visits.FirstAsync();
            visit.SetCost(incorrectCost);

            // Act
            Visit result = await _unitOfWork.VisitRepository.Update(visit);
            await _unitOfWork.CompleteAsync();
        }

        [TestMethod]
        public async Task VisitCount_ShouldReturnCorrectQuantityOfVisitsForUser()
        {
            // Assert 
            Customer customer = (Customer)_context.Users.First();
            int quantityOfVisits = customer.Visits.Count;

            // Act
            int result = await _unitOfWork.VisitRepository.VisitsCountAsync(customer.Email);

            // Assert
            Assert.AreEqual(result, quantityOfVisits);
        }

        [TestMethod]
        [ExpectedException(typeof(ItemNotFoundException))]
        public async Task VisitCout_ShouldThrowItemNotFoundException()
        {
            // Assert
            Customer customer = Customer.Create("name", "lastname", DateTime.Now.AddYears(-20), "53536464", "xyz@gmail.com", 10);
            _context.Users.Add(customer);
            await _context.SaveChangesAsync();

            // Act
            var result = await _unitOfWork.VisitRepository.VisitsCountAsync(customer.Email);
        }

        [TestMethod]
        [ExpectedException(typeof(ItemNotFoundException))]
        public async Task GetByUserAsync_ShouldThrowItemNotFoundException()
        {
            // Assert
            Customer customer = Customer.Create("name", "lastname", DateTime.Now.AddYears(-20), "53536464", "xyz@gmail.com", 10);
            _context.Users.Add(customer);
            await _context.SaveChangesAsync();

            // Act
            List<Visit> result = await _unitOfWork.VisitRepository.GetByUserAsync(customer.Email, 1, 2, "startDate");
        }

        [TestMethod]
        [ExpectedException(typeof(ParseException))]
        public async Task GetByUserAsync_ShouldThrowParseExceptionIfSortParamIncorrect()
        {
            // Arrange
            Customer customer = (Customer)_context.Users.First();

            // Act
            var result = await _unitOfWork.VisitRepository.GetByUserAsync(customer.Email, 1, 2, "incorrect param");
        }

        [TestMethod]
        public async Task GetByUserAsync_ShouldReturnCorrectQuantityOfVisits()
        {
            // Arrange
            var user = await _context.Users.FirstAsync();
            var quantityOfVisits = await _context.Visits.Where(x => x.Customer.Email == user.Email).CountAsync();

            // Act
            var resultOfAllVisits = await _unitOfWork.VisitRepository.GetByUserAsync(user.Email, 1, 50, "startDate");
            var resultOfTwoVisitsOnPage = await _unitOfWork.VisitRepository.GetByUserAsync(user.Email, 1, 2, "startDate");

            // Assert
            Assert.AreEqual(quantityOfVisits, resultOfAllVisits.Count);
            Assert.AreEqual(resultOfTwoVisitsOnPage.Count, 2);
        }

        [TestMethod]
        public async Task Add_ShouldAddVisitIfCorrect()
        {
            // Arrange
            var visitTerm = await _context.VisitTerms.FirstAsync();
            var service = await _context.Services.FirstAsync();
            var user = await _context.Users.FirstAsync();
            Visit visit = Visit.Create(service.Id, visitTerm.Id, user.Id, visitTerm.EndDate.AddHours(-Constants.Constants.VISIT_TIME));

            // Act
            await _unitOfWork.VisitRepository.Add(visit);
            await _unitOfWork.CompleteAsync();

            // Assert
            Assert.AreNotEqual(visit.Id, 0);
            Assert.AreNotEqual(visit.Id, Guid.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(VisitCurrentlyReservedException))]
        public async Task Add_ShouldThrowVisitCurrentlyReservedException()
        {
            // Arrange
            var visit = await _context.Visits.FirstAsync();

            // Act
            await _unitOfWork.VisitRepository.Add(visit);
        }

        [TestMethod]
        [ExpectedException(typeof(ItemNotFoundException))]
        public async Task FindWithCustomerAndServiceAsync_ShouldThrowItemNotFoundExceptionIfItemNotFound()
        {
            // Arrange
            int fakeId = 999;

            // Act
            var result = await _unitOfWork.VisitRepository.FindWithCustomerAndServiceAsync(fakeId);
        }

        [TestMethod]
        public async Task FindWithCustomerAndServiceAsync_ShouldReturnVisitWithServiceAndCustomer()
        {
            // Arrange
            Visit visit = _context.Visits.First();

            // Act
            Visit result = await _unitOfWork.VisitRepository.FindWithCustomerAndServiceAsync(visit.Id);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Customer);
            Assert.IsNotNull(result.Service);
        }

        public void PrepareData()
        {
            var users = GetUsers();

            // Seed users
            foreach (var u in users)
            {
                _context.Users.Add(u);
            }

            // Seed services
            var services = GetServices();
            _context.Services.AddRange(services);

            // Seed visit terms
            var visitTerms = GetVisitTerms();
            _context.VisitTerms.AddRange(visitTerms);

            _context.SaveChanges();

            // Seed visits
            var visits = GetVisits(visitTerms, users, services);
            _context.Visits.AddRange(GetVisits(visitTerms, users, services));
            _context.SaveChanges();
        }

        public IEnumerable<Customer> GetUsers()
        {
            return new List<Customer>
            {
                Customer.Create("name1", "lastname1", DateTime.Now.AddYears(-20), "phonenumber", "name1@wp.pl", 10),
                Customer.Create("name2", "lastname2", DateTime.Now.AddYears(-15), "phonenumber", "name2@wp.pl", 10),
            };
        }

        private IEnumerable<Service> GetServices()
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

        private IEnumerable<VisitTerm> GetVisitTerms()
        {
            return new List<VisitTerm>
            {
                VisitTerm.Create(DateTimeUtil.GetNextWeekday(Constants.Constants.OPENING_HOUR_OF_SALON.AddDays(1), DayOfWeek.Monday)),
                VisitTerm.Create(DateTimeUtil.GetNextWeekday(Constants.Constants.OPENING_HOUR_OF_SALON.AddDays(1), DayOfWeek.Tuesday)),
                VisitTerm.Create(DateTimeUtil.GetNextWeekday(Constants.Constants.OPENING_HOUR_OF_SALON.AddDays(1), DayOfWeek.Wednesday)),
            };
        }

        private IEnumerable<Visit> GetVisits(IEnumerable<VisitTerm> visitTerms, IEnumerable<ApplicationUser> users, IEnumerable<Service> services)
        {
            List<Visit> visits = new List<Visit>();

            foreach (var v in visitTerms)
            {
                DateTime date = v.StartDate;
                foreach (var u in users)
                {
                    date.AddMinutes(Constants.Constants.VISIT_TIME);
                    visits.Add(Visit.Create(1, v.Id, u.Id, date));
                }
            }
            return visits;
        }
    }
}
