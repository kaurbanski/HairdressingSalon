using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WebApplication3.Models
{
    public class Service
    {
        private Service()
        {
            Visits = new List<Visit>();
        }
        public int Id { get; private set; }
        [Required]
        public string Name { get; private set; }
        public virtual ICollection<Visit> Visits { get; }

        public static Service Create(string name)
        {
            return new Service { Name = name };
        }

        public static Service Create(int id, string name)
        {
            return new Service { Id = id, Name = name };
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            Service service = obj as Service;
            if (service == null)
            {
                return false;
            }

            return this.Id == service.Id &&
                this.Name == service.Name;

        }
    }
}