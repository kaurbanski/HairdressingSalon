using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebApplication3.Models;
using WebApplication3.Models.ApiBindingModels;

namespace WebApplication3.Repositories
{
    public interface IServiceRepository : IRepository<Service>
    {
        Task<IEnumerable<Service>> GetAllAsync(string sort);
        Task<List<ServiceStatsBindingModel>> GetAllWithStatsAsync(string sort);
        Task<Service> UpdateAsync(Service service);
        Service Add(Service service);
    }
}
