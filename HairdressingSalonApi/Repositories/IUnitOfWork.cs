using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebApplication3.Repositories
{
    public interface IUnitOfWork : IDisposable
    {
        IServiceRepository ServiceRepository { get; }
        IVisitRepository VisitRepository { get; }
        IVisitTermRepository VisitTermRepository { get; }
        IUserRepository UserRepository { get; }
        Task<int> CompleteAsync();

    }
}
