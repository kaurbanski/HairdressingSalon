using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebApplication3.Models;
using WebApplication3.Models.ApiViewModels;

namespace WebApplication3.Repositories
{
    public interface IUserRepository
    {
        IQueryable<ApplicationUser> GetAll();
        IQueryable<Customer> GetAllCustomersAsIQuerable();
        Task<List<Customer>> GetCustomersAsync(string sort, int page, int pageSize);
        Task<List<ApplicationUser>> GetAllHairdressersAsync(string sort, int page, int pageSize);
        Task<Customer> FindCustomerByEmailAsync(string email);
        Task<int> GetAllCountAsync(string roleName);
        Task<CustomerFullStatsModel> GetCustomerStats(string email);
        Task<List<CustomerStatsApiModel>> GetCustomersWithStats(string sort, int page, int pageSize);
        Task<List<ApplicationUser>> GetUserByPhraseAndRole(string sort, int pageNo, int pageSize, string phrase, string role);
        Task<int> GetByPhraseAndRoleCountAsync(string phrase, string role);
    }
}
