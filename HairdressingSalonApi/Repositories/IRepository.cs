using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebApplication3.Repositories
{
    public interface IRepository<TEntity> where TEntity : class
    {
        Task<TEntity> FindAsync(int id);

        void Remove(TEntity entity);
        Task RemoveById(int id);
        Task ReloadAsync(TEntity entity);
        IQueryable<TEntity> GetAllAsQueryable();
    }
}
