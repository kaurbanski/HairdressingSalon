using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using WebApplication3.Constants;
using WebApplication3.Exceptions;
using WebApplication3.Models;
using WebApplication3.Models.ApiBindingModels;

namespace WebApplication3.Repositories
{
    public class VisitTermRepository : Repository<VisitTerm>, IVisitTermRepository
    {
        public VisitTermRepository(IApplicationDbContext context) : base(context)
        {

        }

        public async Task<bool> IsInDatabase(DateTime date)
        {
            var isInDatabase = await Context.VisitTerms.AnyAsync(x => x.StartDate.Year == date.Year &&
                        x.StartDate.Month == date.Month &&
                        x.StartDate.Day == date.Day);
            return isInDatabase;
        }

        public VisitTerm AddByDate(DateTime date)
        {
            VisitTerm term = VisitTerm.Create(date);
            Context.VisitTerms.Add(term);
            return term;
        }

        public async Task<VisitTerm> AddVisitTermOrReturnIfExists(DateTime date)
        {
            VisitTerm visitTerm = await GetByDate(date);
            if (visitTerm == null)
            {
                visitTerm = AddByDate(date);
            }
            return visitTerm;
        }

        public async Task<VisitTerm> GetByDate(DateTime date)
        {
            return await Context.VisitTerms.FirstOrDefaultAsync(x => x.StartDate.Year == date.Year &&
                       x.StartDate.Month == date.Month &&
                       x.StartDate.Day == date.Day);
        }

        public async Task<VisitTerm> GetByDateWithVisits(DateTime date)
        {
            var result = await Context.VisitTerms
                .Include(x => x.Visits.Select(y => y.Service))
                .Include(x => x.Visits.Select(y => y.Customer))
                .FirstOrDefaultAsync(x => x.StartDate.Year == date.Year &&
                       x.StartDate.Month == date.Month &&
                       x.StartDate.Day == date.Day);

            if (result == null)
            {
                throw new ItemNotFoundException();
            }
            return result;
        }

        public async Task<List<GetVisitTermByDateBindingModel>> GetFreeHours(DateTime date)
        {
            List<GetVisitTermByDateBindingModel> hoursList = new List<GetVisitTermByDateBindingModel>();
            var visitTerm = await GetByDateWithVisits(date);

            if (visitTerm == null)
            {
                throw new ItemNotFoundException();
            }
            var visits = visitTerm.Visits;
            hoursList = DateUtils.GetFreeHoursFromVisits(visits, date);

            return hoursList;
        }
    }
}