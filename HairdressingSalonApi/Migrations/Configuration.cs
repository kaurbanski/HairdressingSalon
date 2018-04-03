namespace WebApplication3.Migrations
{
    using Microsoft.AspNet.Identity;
    using Microsoft.AspNet.Identity.EntityFramework;
    using Models;
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Globalization;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<WebApplication3.Models.ApplicationDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
        }

        protected override void Seed(WebApplication3.Models.ApplicationDbContext context)
        {
            //  This method will be called after migrating to the latest version.

            //  You can use the DbSet<T>.AddOrUpdate() helper extension method 
            //  to avoid creating duplicate seed data. E.g.
            //
            //    context.People.AddOrUpdate(
            //      p => p.FullName,
            //      new Person { FullName = "Andrew Peters" },
            //      new Person { FullName = "Brice Lambson" },
            //      new Person { FullName = "Rowan Miller" }
            //    );
            //

            SeedRoles(context);
            SeedUsers(context);
            seedServices(context);
            //seedVisitTerms(context);
            //seedVisit(context);
        }

        public void SeedRoles(ApplicationDbContext context)
        {
            if (!context.Roles.Any(r => r.Name == "Admin"))
            {
                var store = new RoleStore<IdentityRole>(context);
                var manager = new RoleManager<IdentityRole>(store);
                var role = new IdentityRole { Name = "Admin" };
                manager.Create(role);
            }

            if (!context.Roles.Any(r => r.Name == "Customer"))
            {
                var store = new RoleStore<IdentityRole>(context);
                var manager = new RoleManager<IdentityRole>(store);
                var role = new IdentityRole { Name = "Customer" };
                manager.Create(role);
            }

            if (!context.Roles.Any(r => r.Name == "Hairdresser"))
            {
                var store = new RoleStore<IdentityRole>(context);
                var manager = new RoleManager<IdentityRole>(store);
                var role = new IdentityRole { Name = "Hairdresser" };
                manager.Create(role);
            }
        }

        public void SeedUsers(ApplicationDbContext context)
        {
            ApplicationUserManager manager = new ApplicationUserManager(new UserStore<ApplicationUser>(context));
            if (!context.Users.Any(r => r.Email == "admin@wp.pl"))
            {
                ApplicationUser user = ApplicationUser.Create(
                    "John",
                    "Xyz",
                    DateTime.ParseExact("07-08-1993", "dd-MM-yyyy", System.Globalization.CultureInfo.InvariantCulture),
                    "724-875-098",
                    "admin@wp.pl");

                manager.Create(user, "1qaz2wsx");
                manager.AddToRole(user.Id, "Admin");
            }

            if (!context.Users.Any(r => r.Email == "customer@wp.pl"))
            {
                Customer user = Customer.Create(
                    "Jerzy",
                    "Example",
                    DateTime.ParseExact("07-08-1995", "dd-MM-yyyy", System.Globalization.CultureInfo.InvariantCulture),
                    "513508849",
                    "customer@wp.pl",
                    10);

                manager.Create(user, "1qaz2wsx");
                manager.AddToRole(user.Id, "Customer");
            }
        }

        public void seedServices(ApplicationDbContext context)
        {
            List<Service> services = new List<Service>()
            {
                Service.Create("Wizyta krótsza np. strzy¿enie, czesanie"),
                Service.Create("Wizyta d³u¿sza np. kolo, baleyage"),
                Service.Create("Upiêcia okazjonalne np. fale, loki, upiêcia"),
                Service.Create("Us³ugi ekskluzywne np. kreatynowe prostowanie w³osów, przed³u¿anie w³osów"),
                Service.Create("Baleyage"),
                Service.Create("Grzywka, w¹sy"),
            };

            foreach (var s in services)
            {
                context.Services.AddOrUpdate(s);
            }
        }

        //public void seedVisitTerms(ApplicationDbContext context)
        //{
        //    VisitTerm visitTerm = new VisitTerm();
        //    visitTerm.EndDate = DateTime.ParseExact("17:00", "HH:mm", CultureInfo.InvariantCulture);
        //    visitTerm.StartDate = DateTime.ParseExact("08:00", "HH:mm", CultureInfo.InvariantCulture);
        //    context.VisitTerms.AddOrUpdate(visitTerm);
        //    context.SaveChanges();
        //}

        //public void seedVisit(ApplicationDbContext context)
        //{
        //    Visit visit = new Visit();
        //    visit.CustomerId = "dfd78508-f86b-4dfa-ab89-0414f0f418b8";
        //    visit.ServiceId = context.Services.FirstOrDefault().Id;
        //    visit.StartDate = DateTime.ParseExact("17:00", "HH:mm", CultureInfo.InvariantCulture).AddHours(-1);
        //    context.Visits.AddOrUpdate();
        //    context.SaveChanges();
        //}
    }
}
