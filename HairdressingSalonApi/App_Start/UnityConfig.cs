using AutoMapper;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin.Security;
using NLog;
using System;
using System.Data;
using System.Data.Common;
using System.Data.Entity;
using System.Web;
using Unity;
using Unity.Injection;
using WebApplication3.AutoMapper;
using WebApplication3.Models;
using WebApplication3.Repositories;

namespace WebApplication3
{
    /// <summary>
    /// Specifies the Unity configuration for the main container.
    /// </summary>
    public static class UnityConfig
    {
        #region Unity Container
        private static Lazy<IUnityContainer> container =
          new Lazy<IUnityContainer>(() =>
          {
              var container = new UnityContainer();
              RegisterTypes(container);
              return container;
          });

        /// <summary>
        /// Configured Unity Container.
        /// </summary>
        public static IUnityContainer Container => container.Value;
        #endregion

        /// <summary>
        /// Registers the type mappings with the Unity container.
        /// </summary>
        /// <param name="container">The unity container to configure.</param>
        /// <remarks>
        /// There is no need to register concrete types such as controllers or
        /// API controllers (unless you want to change the defaults), as Unity
        /// allows resolving a concrete type even if it was not previously
        /// registered.
        /// </remarks>
        public static void RegisterTypes(IUnityContainer container)
        {
            // NOTE: To load from web.config uncomment the line below.
            // Make sure to add a Unity.Configuration to the using statements.
            // container.LoadConfiguration();

            // container.RegisterType<IProductRepository, ProductRepository>();
            container.RegisterType<IUserStore<ApplicationUser>, UserStore<ApplicationUser>>();
            container.RegisterType<UserManager<ApplicationUser>>();
            container.RegisterType<IApplicationDbContext, ApplicationDbContext>();
            container.RegisterType<ApplicationUserManager>();
            container.RegisterType<IUserRepository, UserRepository>();
            container.RegisterType<IServiceRepository, ServiceRepository>();
            container.RegisterType<IUnitOfWork, UnitOfWork>();
            container.RegisterType<IServiceRepository, ServiceRepository>();
            container.RegisterType<IVisitRepository, VisitRepository>();
            container.RegisterType<IVisitTermRepository, VisitTermRepository>();
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<MappingProfile>();
            });
            container.RegisterInstance<IMapper>(config.CreateMapper());
            


        }
    }
}