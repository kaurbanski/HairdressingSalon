using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WebApplication3.Models.ApiBindingModels
{
    public class ServiceBindingModel
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public int Id { get; set; }
    }

    public class ServiceStatsBindingModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int QuantityOfVisits { get; set; }
    }
}