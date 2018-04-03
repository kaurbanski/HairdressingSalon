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

namespace WebApplication3.Repositories
{
    public class ServiceRepository : Repository<Service>, IServiceRepository
    {
        public ServiceRepository(IApplicationDbContext context) : base(context)
        {

        }

        public async Task<IEnumerable<Service>> GetAllAsync(string sort)
        {

            var result = await Context.Services.ApplySort(sort).ToListAsync();

            if (result == null || result.Count <= 0)
            {
                throw new ItemNotFoundException();
            }
            return result;
        }

        public async Task<List<ServiceStatsBindingModel>> GetAllWithStatsAsync(string sort)
        {
            var result = await GetAllAsQueryable()
                .Select(x => new ServiceStatsBindingModel { Id = x.Id, Name = x.Name, QuantityOfVisits = x.Visits.Count }).ApplySort(sort).ToListAsync();
            if (result == null || result.Count <=0)
            {
                throw new ItemNotFoundException();
            }
            return result;
        }

        public async Task<Service> UpdateAsync(Service service)
        {
            bool isInDatabase = false;
            isInDatabase = await Context.Services.AnyAsync(x => x.Id == service.Id);
            var serviceFromDb = await Context.Services.FirstOrDefaultAsync(x => x.Id == service.Id);

            if (serviceFromDb == null)
            {
                throw new ItemNotFoundException();
            }

            Context.Entry(serviceFromDb).CurrentValues.SetValues(service);

            return service;
        }

        public Service Add(Service service)
        {
            Context.Services.Add(service);
            return service;
        }
    }
}