using Effort;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebApplication3.Exceptions;
using WebApplication3.Models;
using WebApplication3.Repositories;
using WebApplication3.Tests.DateTimeUtils;

namespace WebApplication3.Tests.Repository
{
    [TestClass]
    public class TestVisitTermRepository
    {
        private ApplicationDbContext _context;
        private IUnitOfWork _unitOfWork;
        private IVisitTermRepository _visitTermRepository;

        [TestInitialize]
        public void SetUp()
        {
            var connection = DbConnectionFactory.CreateTransient();
            _context = new ApplicationDbContext(connection);
            _visitTermRepository = new VisitTermRepository(_context);
            _unitOfWork = new UnitOfWork(_context);
            PrepareData();
        }

        [TestMethod]
        public async Task IsInDatabase_ShouldReturnTrueIfVisitTermExists()
        {
            // Arrange
            VisitTerm visitTerm = await _context.VisitTerms.FirstAsync();

            // Act
            bool isInDatabase = await _unitOfWork.VisitTermRepository.IsInDatabase(visitTerm.StartDate);

            // Assert
            Assert.IsTrue(isInDatabase);
        }

        [TestMethod]
        public async Task IsInDatabase_ShouldReturnFalseIfVisitTermDoesntExists()
        {
            // Arrange
            DateTime fakeDate = DateTime.Now.AddYears(100);

            // Act
            bool isInDatabase = await _unitOfWork.VisitTermRepository.IsInDatabase(fakeDate);

            // Assert
            Assert.IsFalse(isInDatabase);
        }

        [TestMethod]
        public async Task AddByDate_ShouldReturnAddedVisitTermWithNotEmptyId()
        {
            // Arrange
            DateTime date = DateTime.Now.AddYears(1);

            // Act
            var result = _unitOfWork.VisitTermRepository.AddByDate(date);
            await _unitOfWork.CompleteAsync();

            // Assert
            Assert.AreNotEqual(Guid.Empty, result.Id);
        }

        [TestMethod]
        public async Task AddVisitTermOrReturnIfExists_ShouldReturnExistingEntityFromDate()
        {
            // Arrange
            VisitTerm visitTerm = await _context.VisitTerms.FirstAsync();

            // Act
            VisitTerm result = await _unitOfWork.VisitTermRepository.AddVisitTermOrReturnIfExists(visitTerm.StartDate);

            // Assert
            Assert.AreEqual(result.Id, visitTerm.Id);
        }

        [TestMethod]
        public async Task AddVisitTermOrReturnIfExists_ShouldAddVisitTermIfEntityWithDateDoesntExists()
        {
            // Arrange
            DateTime date = DateTime.Now.AddYears(1);
            int visitTermCounterBeforeMethodExecution = await _context.VisitTerms.CountAsync();

            // Act
            var result = await _unitOfWork.VisitTermRepository.AddVisitTermOrReturnIfExists(date);
            await _unitOfWork.CompleteAsync();

            // Assert
            Assert.AreNotEqual(Guid.Empty, result.Id);
            Assert.AreEqual(visitTermCounterBeforeMethodExecution + 1, await _context.VisitTerms.CountAsync());
        }

        [TestMethod]
        [ExpectedException(typeof(DbEntityValidationException))]
        public async Task AddVisitTermOrReturnIfExist_ShouldThrowDbEntityValidationExceptionIfDateFromThePast()
        {
            // Arrange
            DateTime dateFromPast = DateTime.Now.AddYears(-1);

            // Act
            await _unitOfWork.VisitTermRepository.AddVisitTermOrReturnIfExists(dateFromPast);
            await _unitOfWork.CompleteAsync();
        }

        [TestMethod]
        public async Task GetByDate_ShouldRetunVisitTermFromDateIfVisitTermExist()
        {
            // Arrange
            var visitTerm = await _context.VisitTerms.FirstAsync();

            // Act
            var result = await _unitOfWork.VisitTermRepository.GetByDate(visitTerm.EndDate);

            // Assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public async Task GetByDate_ShouldReturnNullIfVisitTermWithDateDoesntExist()
        {
            // Arrange
            DateTime date = DateTime.Now.AddYears(1);

            // Act
            var result = await _unitOfWork.VisitTermRepository.GetByDate(date);

            // Assert
            Assert.IsNull(result);
        }

        [TestMethod]
        [ExpectedException(typeof(ItemNotFoundException))]
        public async Task GetByDateWithVisits_ShouldThrowItemNotFoundExceptionIfItemNotFound()
        {
            // Arrange
            DateTime date = DateTime.Now.AddYears(1);

            // Act
            var result = await _unitOfWork.VisitTermRepository.GetByDateWithVisits(date);
        }

        [TestMethod]
        public async Task GetByDateWithVisits_ShouldReturnVisitTermWithVisit()
        {
            // Arrange
            VisitTerm visitTerm = await _context.VisitTerms.FirstAsync();

            // Act
            VisitTerm result = await _unitOfWork.VisitTermRepository.GetByDateWithVisits(visitTerm.EndDate);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Visits);
        }

        [TestMethod]
        [ExpectedException(typeof(ItemNotFoundException))]
        public async Task GetFreeHours_ShouldReturnItemNotFoundException()
        {
            // Arrange
            DateTime date = DateTime.Now.AddYears(1);

            // Act
            var result = await _unitOfWork.VisitTermRepository.GetFreeHours(date);
        }

        [TestMethod]
        public async Task GetFreeHours_ShouldReturnAppropriateCountOfFreeHours()
        {
            // Arrange
            VisitTerm visitTerm = await _context.VisitTerms.FirstAsync();

            // Act
            var result = await _unitOfWork.VisitTermRepository.GetFreeHours(visitTerm.EndDate);

            // Assert
            Assert.AreEqual(18 - visitTerm.Visits.Count, result.Count);
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
                    date = date.AddMinutes(Constants.Constants.VISIT_TIME);
                    visits.Add(Visit.Create(1, v.Id, u.Id, date));
                }
            }
            return visits;
        }
    }
}
