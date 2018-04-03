using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using WebApplication3.Exceptions;
using WebApplication3.Helpers;
using WebApplication3.Models;

namespace WebApplication3.Repositories
{
    public class VisitRepository : Repository<Visit>, IVisitRepository
    {
        public VisitRepository(IApplicationDbContext context) : base(context)
        {

        }

        public async Task<bool> IsReserved(DateTime startDate)
        {
            bool result = false;
            result = await Context.Visits.AnyAsync(x => x.StartDate == startDate);
            return result;
        }

        public async Task<Visit> Update(Visit visit)
        {
            var originalEntity = await FindWithCustomerAndServiceAsync(visit.Id);
            var customerDiscount = visit.Customer.PercentDiscount;
            var costAfterDiscount = Constants.CostUtils.GetCostOfTheVisitAfterDiscount(visit.Cost, customerDiscount);
            visit.SetCostAfterDiscount(costAfterDiscount);
            return visit;
        }

        public async Task<int> VisitsCountAsync(string email)
        {
            int quantity = 0;
            quantity = await GetAllAsQueryable().Where(x => x.Customer.Email == email).CountAsync();

            if (quantity < 1)
            {
                throw new ItemNotFoundException();
            }
            return quantity;
        }

        public async Task<List<Visit>> GetByUserAsync(string email, int pageNo, int pageSize, string sort)
        {
            var result = await GetAllAsQueryable().Include(x => x.Service).Where(x => x.Customer.Email == email).ApplySort(sort).Skip((pageNo - 1) * pageSize).Take(pageSize).ToListAsync();
            if (result == null || result.Count <= 0)
            {
                throw new ItemNotFoundException();
            }
            return result;
        }

        public async Task<Visit> Add(Visit visit)
        {
            bool reserved = true;
            reserved = await IsReserved(visit.StartDate);
            if (reserved)
            {
                throw new VisitCurrentlyReservedException();
            }
            var result = Context.Visits.Add(visit);
            return result;
        }

        public async Task<Visit> FindWithCustomerAndServiceAsync(int id)
        {
            Visit visit = await Context.Visits.Include(x => x.Customer).Include(x => x.Service).FirstOrDefaultAsync(x => x.Id == id);

            if (visit == null)
            {
                throw new ItemNotFoundException();
            }

            return visit;
        }
    }
}