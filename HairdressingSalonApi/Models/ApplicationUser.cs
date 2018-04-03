using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;

namespace WebApplication3.Models
{
    public class ApplicationUser : IdentityUser
    {
        protected ApplicationUser()
        {

        }
        [Required]
        public string FirstName { get; protected set; }
        [Required]
        public string LastName { get; protected set; }
        [DataType(DataType.Date)]
        [Required]
        public DateTime BirthDate { get; protected set; }
        [Required]
        public string Phone { get; protected set; }

        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager, string authenticationType)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, authenticationType);
            // Add custom user claims here
            return userIdentity;
        }

        public static ApplicationUser Create(string firstName, string lastName, DateTime birthDate, string Phone, string email)
        {
            return new ApplicationUser
            {
                FirstName = firstName,
                LastName = lastName,
                BirthDate = birthDate,
                Phone = Phone,
                Email = email,
                UserName = email,
            };
        }
    }
}