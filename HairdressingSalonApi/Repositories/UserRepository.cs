using AutoMapper;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using WebApplication3.Exceptions;
using WebApplication3.Helpers;
using WebApplication3.Models;
using WebApplication3.Models.ApiBindingModels;
using WebApplication3.Models.ApiViewModels;

namespace WebApplication3.Repositories
{
    public class UserRepository : Repository<ApplicationUser>, IUserRepository
    {
        public UserRepository(IApplicationDbContext context) : base(context)
        {
        }

        public IQueryable<ApplicationUser> GetAll()
        {
            return Context.Users.AsQueryable();
        }

        public IQueryable<Customer> GetAllCustomersAsIQuerable()
        {
            return Context.Users.OfType<Customer>();
        }

        public async Task<Customer> FindCustomerByEmailAsync(string email)
        {
            var user = await Context.Users.OfType<Customer>().FirstOrDefaultAsync(x => x.Email == email);
            if (user == null)
            {
                throw new ItemNotFoundException();
            }
            return user;
        }

        public async Task<List<Customer>> GetCustomersAsync(string sort, int page, int pageSize)
        {
            var customers = await GetAllCustomersAsIQuerable()
                .ApplySort(sort)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            if (customers == null || customers.Count == 0)
            {
                throw new ItemNotFoundException();
            }
            return customers;
        }

        public async Task<List<ApplicationUser>> GetAllHairdressersAsync(string sort, int page, int pageSize)
        {
            var store = new RoleStore<IdentityRole>((ApplicationDbContext)Context);
            var manager = new RoleManager<IdentityRole>(store);
            var role = manager.Roles.FirstOrDefault(r => r.Name == "Hairdresser");
            var hairdressers = await GetAll().Where(u => u.Roles.Any(r => r.RoleId == role.Id))
                .ApplySort(sort)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            if (hairdressers.Count <= 0)
            {
                throw new ItemNotFoundException();
            }

            return hairdressers;
        }

        public async Task<int> GetAllCountAsync(string roleName)
        {
            var store = new RoleStore<IdentityRole>((ApplicationDbContext)Context);
            var manager = new RoleManager<IdentityRole>(store);
            var role = await manager.FindByNameAsync(roleName);
            return await GetAll().Where(r => r.Roles.Any(x => x.RoleId == role.Id)).CountAsync();
        }

        public async Task<CustomerFullStatsModel> GetCustomerStats(string email)
        {
            CustomerFullStatsModel model = new CustomerFullStatsModel();

            var customer = await Context.Users.OfType<Customer>().Where(x => x.Email == email).Include(x => x.Visits.Select(y => y.Service)).FirstOrDefaultAsync();

            if (customer == null)
            {
                throw new ItemNotFoundException();
            }

            model.Customer = customer;

            var visitsFromThePast = customer.Visits.Where(x => x.StartDate < DateTime.Now);

            if (visitsFromThePast.Count() > 0)
            {
                model.Stats = new CustomerStatsModel
                {
                    NumberOfVisits = visitsFromThePast.Count(),
                    LowestCostOfTheVisit = visitsFromThePast.Min(x => x.CostAfterDiscount),
                    HighestCostOfTheVisit = visitsFromThePast.Max(x => x.CostAfterDiscount),
                    AverageCostOfTheVisit = Math.Round(visitsFromThePast.Average(x => x.CostAfterDiscount), 0),
                };


                var service = visitsFromThePast.GroupBy(x => x.Service).OrderByDescending(x => x.Count()).Select(x => x.Key).FirstOrDefault();
                model.MostPopularService = service;
            }
            return model;
        }

        public async Task<List<CustomerStatsApiModel>> GetCustomersWithStats(string sort, int page, int pageSize)
        {
            var customers = await GetAllCustomersAsIQuerable()
                .Include(x => x.Visits)
                .Select(x =>
                            new CustomerStatsApiModel
                            {
                                Customer = new CustomerInfoBindingModel
                                {
                                    BirthDate = x.BirthDate,
                                    Email = x.Email,
                                    FirstName = x.FirstName,
                                    LastName = x.LastName,
                                    PercentDiscount = x.PercentDiscount,
                                    Phone = x.Phone
                                },
                                MostPopularService = (x.Visits.Any()) ? x.Visits.GroupBy(y => y.Service).OrderByDescending(y => y.Count()).Select(y => new ServiceBindingModel { Id = y.Key.Id, Name = y.Key.Name }).FirstOrDefault() : null,
                                Stats = new CustomerStatsModel
                                {
                                    AverageCostOfTheVisit = Math.Round((x.Visits.Any()) ? x.Visits.Average(y => y.CostAfterDiscount) : 0, 0),
                                    HighestCostOfTheVisit = (x.Visits.Any()) ? x.Visits.Max(y => y.CostAfterDiscount) : 0,
                                    LowestCostOfTheVisit = (x.Visits.Any()) ? x.Visits.Min(y => y.CostAfterDiscount) : 0,
                                    NumberOfVisits = (x.Visits.Any()) ? x.Visits.Count : 0,
                                }
                            })
                .ApplySort(sort)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            if (customers == null)
            {
                throw new ItemNotFoundException();
            }
            return customers;
        }

        public async Task<List<ApplicationUser>> GetUserByPhraseAndRole(string sort, int pageNo, int pageSize, string phrase, string roleName)
        {
            var store = new RoleStore<IdentityRole>((ApplicationDbContext)Context);
            var manager = new RoleManager<IdentityRole>(store);
            var role = await manager.FindByNameAsync(roleName);
            List<ApplicationUser> users = null;

            users = await GetAll()
                .Where(x => x.FirstName.Contains(phrase) || x.LastName.Contains(phrase) || x.Email.Contains(phrase))
                .Where(x => x.Roles.Any(y => y.RoleId == role.Id))
                .ApplySort(sort)
                .Skip((pageNo - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            if (users == null || users.Count <= 0)
            {
                throw new ItemNotFoundException();
            }
            return users;
        }

        public async Task<int> GetByPhraseAndRoleCountAsync(string phrase, string roleName)
        {
            var store = new RoleStore<IdentityRole>((ApplicationDbContext)Context);
            var manager = new RoleManager<IdentityRole>(store);
            var role = await manager.FindByNameAsync(roleName);
            int result = 0;

            result = await GetAll()
                .Where(x => x.FirstName.Contains(phrase) || x.LastName.Contains(phrase) || x.Email.Contains(phrase))
                .Where(x => x.Roles.Any(y => y.RoleId == role.Id)).CountAsync();

            if (result <= 0)
            {
                throw new ItemNotFoundException();
            }
            return result;
        }
    }
}