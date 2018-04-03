using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Data.Common;
using Unity.Attributes;

namespace WebApplication3.Models
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>, IApplicationDbContext
    {

        [InjectionConstructor]
        public ApplicationDbContext()
            : base("name=DefaultConnection")
        {
        }

        public ApplicationDbContext(DbConnection connection) : base(connection, true)
        {

        }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }

        public virtual DbSet<Service> Services { get; set; }
        public virtual DbSet<Visit> Visits { get; set; }
        public virtual DbSet<VisitTerm> VisitTerms { get; set; }
    }
}