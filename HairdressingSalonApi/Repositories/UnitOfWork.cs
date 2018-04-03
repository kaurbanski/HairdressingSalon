using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using WebApplication3.Models;

namespace WebApplication3.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly IApplicationDbContext _context;
        public IServiceRepository ServiceRepository { get; private set; }
        public IVisitRepository VisitRepository { get; private set; }
        public IVisitTermRepository VisitTermRepository { get; private set; }
        public IUserRepository UserRepository { get; private set; }

        public UnitOfWork(IApplicationDbContext context)
        {
            _context = context;
            ServiceRepository = new ServiceRepository(_context);
            VisitRepository = new VisitRepository(_context);
            VisitTermRepository = new VisitTermRepository(_context);
            UserRepository = new UserRepository(_context);
        }


        public async Task<int> CompleteAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}