using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebApplication3.Models;
using WebApplication3.Models.ApiBindingModels;

namespace WebApplication3.Repositories
{
    public interface IVisitTermRepository : IRepository<VisitTerm>
    {
        Task<bool> IsInDatabase(DateTime date);
        VisitTerm AddByDate(DateTime date);
        Task<VisitTerm> GetByDate(DateTime date);
        Task<VisitTerm> GetByDateWithVisits(DateTime date);
        Task<List<GetVisitTermByDateBindingModel>> GetFreeHours(DateTime date);
        Task<VisitTerm> AddVisitTermOrReturnIfExists(DateTime date);

    }
}
