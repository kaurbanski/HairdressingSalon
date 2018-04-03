using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using WebApplication3.Exceptions;
using WebApplication3.Models;

namespace WebApplication3.Repositories
{
    public class Repository<TEntity> : IRepository<TEntity> where TEntity : class
    {
        protected readonly ApplicationDbContext Context;
        protected readonly DbSet<TEntity> DbSet;

        public Repository(IApplicationDbContext context)
        {
            Context = (ApplicationDbContext)context;
            DbSet = Context.Set<TEntity>();
        }

        public async Task<TEntity> FindAsync(int id)
        {
            var entity = await Context.Set<TEntity>().FindAsync(id);
            if (entity == null)
            {
                throw new ItemNotFoundException();
            }

            return entity;
        }

        public void Remove(TEntity entity)
        {
            Context.Set<TEntity>().Remove(entity);
        }

        public async Task RemoveById(int id)
        {
            var entity = await FindAsync(id);
            Context.Set<TEntity>().Remove(entity);
        }

        public async Task ReloadAsync(TEntity entity)
        {
            await Context.Entry<TEntity>(entity).ReloadAsync();
        }

        public IQueryable<TEntity> GetAllAsQueryable()
        {
            var result = Context.Set<TEntity>().AsQueryable();

            return result;
        }
    }

}