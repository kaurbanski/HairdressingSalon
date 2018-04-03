using Effort;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Dynamic;
using System.Text;
using System.Threading.Tasks;
using WebApplication3.Exceptions;
using WebApplication3.Models;
using WebApplication3.Repositories;
using WebApplication3.Tests.DateTimeUtils;

namespace WebApplication3.Tests.Repository
{
    [TestClass]
    public class TestUserRepository
    {
        private ApplicationDbContext _context;
        private IUserRepository _userRepository;
        private IUnitOfWork _unitOfWork;

        [TestInitialize]
        public void SetUp()
        {
            var connection = DbConnectionFactory.CreateTransient();
            _context = new ApplicationDbContext(connection);
            _userRepository = new UserRepository(_context);
            _unitOfWork = new UnitOfWork(_context);
            PrepareData();
        }

        [TestMethod]
        public async Task GetAll_ShouldReturnAllUsers()
        {
            // Arrange
            int counter = await _context.Users.CountAsync();

            // Act
            var result = _unitOfWork.UserRepository.GetAll();

            // Assert
            Assert.AreEqual(await result.CountAsync(), counter);
            Assert.IsInstanceOfType(result, typeof(IQueryable<ApplicationUser>));
        }

        [TestMethod]
        public async Task GetAllCustomersAsIQueryable_ShouldReturnAllCustomers()
        {
            // Arrange
            int counter = await _context.Users.OfType<Customer>().CountAsync();

            // Act
            var result = _unitOfWork.UserRepository.GetAllCustomersAsIQuerable();

            // Assert
            Assert.AreEqual(await result.CountAsync(), counter);
            Assert.IsInstanceOfType(result, typeof(IQueryable<Customer>));
        }
        [TestMethod]
        [ExpectedException(typeof(ItemNotFoundException))]
        public async Task FindCustomerByEmailAsync_ShouldThrowItemNotFoundExceptionIfItemNotFound()
        {
            // Act
            var result = await _unitOfWork.UserRepository.FindCustomerByEmailAsync("fakeemail");
        }

        [TestMethod]
        public async Task FindCustomerByEmailAsync_ShouldReturnCustomer()
        {
            // Arrange
            Customer customer = await _context.Users.OfType<Customer>().FirstAsync();

            // Act
            var result = await _unitOfWork.UserRepository.FindCustomerByEmailAsync(customer.Email);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(Customer));
        }

        [TestMethod]
        [ExpectedException(typeof(ItemNotFoundException))]
        public async Task GetCustomersAsync_ShouldThrowItemNotFoundExceptionIfUsersNotFound()
        {
            // Arrange
            var users = await _context.Users.ToListAsync();
            foreach (var u in users)
            {
                _context.Users.Remove(u);
            }
            await _context.SaveChangesAsync();

            // Act
            var result = await _unitOfWork.UserRepository.GetCustomersAsync("firstName", 1, 5);
        }

        [TestMethod]
        [ExpectedException(typeof(ParseException))]
        public async Task GetCustomersAsync_ShouldThrowParseExceptionIfSortParamIncorrect()
        {
            // Arrange
            string sortParam = "incorrect_sort_param";

            // Act
            var result = await _unitOfWork.UserRepository.GetCustomersAsync(sortParam, 1, 1);
        }

        [TestMethod]
        public async Task GetCustomersAsync_ShouldReturnCustomersSortedAndWithAppropriateQuantity()
        {
            // Arrange
            string firstNameParam = "firstName";
            string lastNameParam = "lastName";
            string emailParam = "email";

            // Act
            var threeCustomersForOnePageSortedByFirstName = await _unitOfWork.UserRepository.GetCustomersAsync(firstNameParam, 1, 3);
            var threeCustomerForSecondPageSortedByLastName = await _unitOfWork.UserRepository.GetCustomersAsync(lastNameParam, 2, 3);
            var twoCustomersForThirdPageSortedByEmail = await _unitOfWork.UserRepository.GetCustomersAsync(emailParam, 3, 3);

            // Assert
            Assert.AreEqual(threeCustomersForOnePageSortedByFirstName.Count, 3);
            CollectionAssert.AreEqual(threeCustomersForOnePageSortedByFirstName.OrderBy(x => x.FirstName).ToList(), threeCustomersForOnePageSortedByFirstName);
            Assert.AreEqual(threeCustomerForSecondPageSortedByLastName.Count, 3);
            CollectionAssert.AreEqual(threeCustomerForSecondPageSortedByLastName.OrderBy(x => x.LastName).ToList(), threeCustomerForSecondPageSortedByLastName);
            Assert.AreEqual(twoCustomersForThirdPageSortedByEmail.Count, 2);
            CollectionAssert.AreEqual(twoCustomersForThirdPageSortedByEmail.OrderBy(x => x.Email).ToList(), twoCustomersForThirdPageSortedByEmail);
        }

        [TestMethod]
        [ExpectedException(typeof(ParseException))]
        public async Task GetAllHairdressersAsync_ShouldThrowParseExceptionIfSortParamIncorrect()
        {
            // Arrange
            string sortParam = "incorrect_sort_param";

            // Act
            var result = await _unitOfWork.UserRepository.GetAllHairdressersAsync(sortParam, 1, 1);
        }

        [TestMethod]
        [ExpectedException(typeof(ItemNotFoundException))]
        public async Task GetAllHairdressersAsync_ShouldThrowItemNotFoundExceptionIfUsersNotFound()
        {
            // Arrange
            var users = _context.Users;
            foreach (var user in users)
            {
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
            }

            // Act
            var result = await _unitOfWork.UserRepository.GetAllHairdressersAsync("firstName", 1, 1);
        }

        [TestMethod]
        public async Task GetAllHairdressersAsync_ShouldReturnHairdressersSortedAndWithAppropriateQuantity()
        {
            // Arrange
            string firstNameParam = "firstName";
            string lastNameParam = "lastName";

            // Act
            var threeHairdressersForFirstPageSortedByFirstName = await _unitOfWork.UserRepository.GetAllHairdressersAsync(firstNameParam, 1, 3);
            var twoHaidressersForThirdPageSortedByLasName = await _unitOfWork.UserRepository.GetAllHairdressersAsync(lastNameParam, 3, 2);

            // Assert
            Assert.AreEqual(3, threeHairdressersForFirstPageSortedByFirstName.Count);
            CollectionAssert.AreEqual(threeHairdressersForFirstPageSortedByFirstName.OrderBy(x => x.FirstName).ToList(), threeHairdressersForFirstPageSortedByFirstName);
            Assert.AreEqual(2, twoHaidressersForThirdPageSortedByLasName.Count);
            CollectionAssert.AreEqual(twoHaidressersForThirdPageSortedByLasName.OrderBy(x => x.LastName).ToList(), twoHaidressersForThirdPageSortedByLasName);
        }

        [TestMethod]
        public async Task GetAllCountAsync_ShouldReturnAppropriateUsersCount()
        {
            // Arrange
            int customerQuantity = GetCustomers().Count();
            int haidressersQuantity = GetHairdressers().Count();

            // Act 
            var result = _context.Users.ToList();
            int customersResult = await _unitOfWork.UserRepository.GetAllCountAsync("Customer");
            int hairdresserResult = await _unitOfWork.UserRepository.GetAllCountAsync("Hairdresser");

            // Assert
            Assert.AreEqual(customerQuantity, customersResult);
            Assert.AreEqual(haidressersQuantity, hairdresserResult);
        }

        [TestMethod]
        [ExpectedException(typeof(ItemNotFoundException))]
        public async Task GetCustomerStats_ShouldReturnItemNotFoundExceptionIfUserNotFound()
        {
            // Arrange
            string fakeEmail = "fakeemail";

            // Act
            var result = await _unitOfWork.UserRepository.GetCustomerStats(fakeEmail);
        }

        [TestMethod]
        [ExpectedException(typeof(ItemNotFoundException))]
        public async Task GetUserByPhraseAndRole_ThrowItemNotFoundExceptonIfUserNotFound()
        {
            // Arrange
            string phrase = "fake phrase";

            // Act
            var result = await _unitOfWork.UserRepository.GetUserByPhraseAndRole("firstName", 1, 1, phrase, "Customer");
        }

        [TestMethod]
        [ExpectedException(typeof(ParseException))]
        public async Task GetUserByPhradeAndRole_ThrowParseExceptionIfSortParamIncorrect()
        {
            // Arrange
            string phrase = "a";
            string sortParam = "incorrect sort param";

            // Act
            var result = await _unitOfWork.UserRepository.GetUserByPhraseAndRole(sortParam, 1, 1, phrase, "Customer");
        }

        [TestMethod]
        public async Task GetUserByPhradeAndRole_ShouldReturnUsersWithPhrase()
        {
            // Arrange
            string phrase = "phrase";
            string sortParam = "firstName";

            // Act
            var res = _context.Users.ToList();
            var result = await _unitOfWork.UserRepository.GetUserByPhraseAndRole(sortParam, 1, 5, phrase, "Customer");

            // Assert
            Assert.AreEqual(2, result.Count);
        }

        [TestMethod]
        [ExpectedException(typeof(ItemNotFoundException))]
        public async Task GetByPhraseAndRoleCountAsync_ShouldThrowItemNotFoundExceptionIfCountIsZero()
        {
            // Arrange
            string phrase = "fake phrase";

            // Act
            var result = await _unitOfWork.UserRepository.GetByPhraseAndRoleCountAsync(phrase, "Customer");
        }

        [TestMethod]
        public async Task GetByPhraseAndRoleCountAsync_ReturnCorrectNumberOfUsersWithPhrase()
        {
            // Arrange
            string phrase = "phrase";

            // Act
            var result = await _unitOfWork.UserRepository.GetByPhraseAndRoleCountAsync(phrase, "Customer");

            // Assert
            Assert.AreEqual(2, result);
        }

        public void PrepareData()
        {
            var customers = GetCustomers();

            // Seed roles
            PrepareRoles();

            var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(_context));

            // Seed customers
            foreach (var u in customers)
            {
                userManager.CreateAsync(u, "1qaz@WSX");
                userManager.AddToRoleAsync(u.Id, "Customer");
            }

            // Seed services
            var services = GetServices();
            _context.Services.AddRange(services);

            // Seed visit terms
            var visitTerms = GetVisitTerms();
            _context.VisitTerms.AddRange(visitTerms);

            _context.SaveChanges();

            // Seed visits
            var visits = GetVisits(visitTerms, customers, services);

            _context.Visits.AddRange(GetVisits(visitTerms, customers, services));
            _context.SaveChanges();

            // Seed Haidresser Users
            var users = GetHairdressers();

            foreach (var u in users)
            {
                userManager.CreateAsync(u, "1qaz@WSX");
                userManager.AddToRoleAsync(u.Id, "Hairdresser");
            }

        }

        public IEnumerable<Customer> GetCustomers()
        {
            return new List<Customer>
            {
                Customer.Create("name1", "gggg", DateTime.Now.AddYears(-20), "phonenumber", "name1@wp.pl", 10),
                Customer.Create("name2", "lastname2", DateTime.Now.AddYears(-15), "phonenumber", "name2@wp.pl", 10),
                Customer.Create("name3", "lastname5", DateTime.Now.AddYears(-15), "phonenumber", "name3@wp.pl", 10),
                Customer.Create("name4", "phrase", DateTime.Now.AddYears(-15), "phonenumber", "name4@wp.pl", 10),
                Customer.Create("name5", "lastname1", DateTime.Now.AddYears(-20), "phonenumber", "name5@wp.pl", 10),
                Customer.Create("name6", "lastname2", DateTime.Now.AddYears(-15), "phonenumber", "name6@wp.pl", 10),
                Customer.Create("name7", "lastname5", DateTime.Now.AddYears(-15), "phonenumber", "name7@wp.pl", 10),
                Customer.Create("phrase", "lastname6", DateTime.Now.AddYears(-15), "phonenumber", "name8@wp.pl", 10),
            };
        }

        public IEnumerable<ApplicationUser> GetHairdressers()
        {
            return new List<ApplicationUser>
            {
               ApplicationUser.Create("name9", "lastname3", DateTime.Now.AddYears(-17), "43535454", "name9@wp.pl"),
               ApplicationUser.Create("name10", "lastname4", DateTime.Now.AddYears(-16),"553564", "name10@wp.pl"),
               ApplicationUser.Create("name11", "lastname3", DateTime.Now.AddYears(-17), "43535454", "name11@wp.pl"),
               ApplicationUser.Create("name12", "lastname4", DateTime.Now.AddYears(-16),"553564", "name12@wp.pl"),
               ApplicationUser.Create("name13", "lastname3", DateTime.Now.AddYears(-17), "43535454", "name13@wp.pl"),
               ApplicationUser.Create("name10", "lastname4", DateTime.Now.AddYears(-16),"553564", "name14@wp.pl"),
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

        private IEnumerable<Visit> GetVisits(IEnumerable<VisitTerm> visitTerms, IEnumerable<Customer> users, IEnumerable<Service> services)
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

        public void PrepareRoles()
        {
            var store = new RoleStore<IdentityRole>(_context);
            var manager = new RoleManager<IdentityRole>(store);
            var customer = new IdentityRole { Name = "Customer" };
            var hairdresser = new IdentityRole { Name = "Hairdresser" };
            manager.Create(customer);
            manager.Create(hairdresser);
        }

        //public void PrepareVisitsFromPast(string userEmail, int serviceId)
        //{
        //    VisitTerm visitTerm = VisitTerm.Create(DateTimeUtil.GetNextWeekday(Constants.Constants.OPENING_HOUR_OF_SALON.AddDays(-21), DayOfWeek.Monday));
        //    VisitTerm visitTerm2 = VisitTerm.Create(DateTimeUtil.GetNextWeekday(Constants.Constants.OPENING_HOUR_OF_SALON.AddDays(-21), DayOfWeek.Tuesday));
        //    _context.VisitTerms.Add(visitTerm);
        //    _context.VisitTerms.Add(visitTerm2);
        //    _context.SaveChanges();

        //    Service service = _context.Services.First(x => x.Id == serviceId);
        //    Customer customer = _context.Users.OfType<Customer>().First(x => x.Email == userEmail);
        //    Visit visit = Visit.Create(service.Id, visitTerm.Id, customer.Id, visitTerm.StartDate);
        //    visit.SetCostAfterDiscount(50);
        //    visit.SetCostAfterDiscount(100);
        //    Visit visit2 = Visit.Create(service.Id, visitTerm2.Id, customer.Id, visitTerm2.StartDate);
        //    _context.Visits.Add(visit);
        //    _context.Visits.Add(visit2);
        //    _context.SaveChanges();
        //}
    }
}
