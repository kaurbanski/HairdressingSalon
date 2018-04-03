using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WebApplication3.Models
{
    public class Customer : ApplicationUser
    {
        private Customer()
        {
            PercentDiscount = 0;
            Visits = new List<Visit>();
        }

        [Range(0, 30, ErrorMessage = "Maximum discount is {0}")]
        public int PercentDiscount { get; private set; }
        public virtual ICollection<Visit> Visits { get; }

        public void SetDiscount(int discount)
        {
            PercentDiscount = discount;
        }

        public static Customer Create(string firstName, string lastName, DateTime birthDate, string phone, string email, int percentDiscount)
        {
            return new Customer { FirstName = firstName, LastName = lastName, BirthDate = birthDate, Phone = phone, Email = email, UserName = email, PercentDiscount = percentDiscount };
        }

        public static Customer Create(string id, string firstName, string lastName, DateTime birthDate, string phone, string email, int percentDiscount)
        {
            return new Customer { Id = id, FirstName = firstName, LastName = lastName, BirthDate = birthDate, Phone = phone, Email = email, UserName = email, PercentDiscount = percentDiscount };
        }
    }
}