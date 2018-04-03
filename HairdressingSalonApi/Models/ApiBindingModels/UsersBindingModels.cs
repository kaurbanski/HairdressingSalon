using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using WebApplication3.AutoMapper;
using WebApplication3.Models.ApiBindingModels;

namespace WebApplication3.Models.ApiViewModels
{
    public class UserInfoBindingModel
    {
        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }
        [Required]
        [JsonConverter(typeof(DateWithHourTimeConverter))]
        public DateTime BirthDate { get; set; }
        [Required]
        [Phone]
        public string Phone { get; set; }
        [EmailAddress]
        public string Email { get; set; }
    }

    public class CustomerInfoBindingModel : UserInfoBindingModel
    {
        public int PercentDiscount { get; set; }
    }

    public class UserAddBindingModel : UserInfoBindingModel
    {
        [DataType(DataType.Password)]
        [Required]
        [MinLength(6)]
        public string Password { get; set; }
    }

    public class CustomerSetDiscountModel
    {
        [EmailAddress]
        public string Email { get; set; }
        [Range(0, 30)]
        public int PercentDiscount { get; set; }
    }

    public class CustomerStatsModel
    {
        public int NumberOfVisits { get; set; }
        public decimal LowestCostOfTheVisit { get; set; }
        public decimal HighestCostOfTheVisit { get; set; }
        public decimal AverageCostOfTheVisit { get; set; }
    }

    public class CustomerFullStatsModel
    {
        public Customer Customer { get; set; }
        public Service MostPopularService { get; set; }
        public CustomerStatsModel Stats { get; set; }
    }

    public class CustomerStatsApiModel
    {
        public CustomerInfoBindingModel Customer { get; set; }
        public ServiceBindingModel MostPopularService { get; set; }
        public CustomerStatsModel Stats { get; set; }
    }
}