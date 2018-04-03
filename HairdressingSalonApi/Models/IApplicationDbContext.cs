using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace WebApplication3.Models
{
    public interface IApplicationDbContext
    {
        DbSet<Service> Services { get; set; }
        DbSet<Visit> Visits { get; set; }
        DbSet<VisitTerm> VisitTerms { get; set; }
        int SaveChanges();

        DbEntityEntry Entry(object entity);
        Task<int> SaveChangesAsync();
        void Dispose();
    }
}