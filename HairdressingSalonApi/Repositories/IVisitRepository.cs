using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebApplication3.Models;

namespace WebApplication3.Repositories
{
    public interface IVisitRepository : IRepository<Visit>
    {
        Task<bool> IsReserved(DateTime startDate);
        Task<Visit> Update(Visit visit);
        Task<int> VisitsCountAsync(string email);
        Task<List<Visit>> GetByUserAsync(string email, int pageNo, int pageSize, string sort);
        Task<Visit> Add(Visit visit);
        Task<Visit> FindWithCustomerAndServiceAsync(int id);
    }
}
