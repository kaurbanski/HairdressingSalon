using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using WebApplication3.AutoMapper;
using WebApplication3.Models.ApiViewModels;
using WebApplication3.Models.Validation.Attributes;

namespace WebApplication3.Models.ApiBindingModels
{
    public class AddVisitBindingModel
    {
        [Required]
        [DataType(DataType.DateTime)]
        [IsTheDateFromFuture(ErrorMessage = "Date must be from future!")]
        [JsonConverter(typeof(DateWithHourTimeConverter))]
        [VisitStartDate(ErrorMessage = "Date and time from outside the salon hours!")]
        public DateTime StartDate { get; set; }
        public int ServiceId { get; set; }
    }

    public class FreeHoursBindingModel
    {
        [Required]
        [DataType(DataType.DateTime)]
        [IsTheDateTodayOrFromFuture(ErrorMessage = "Date must be from future!")]
        [JsonConverter(typeof(DateTimeConverter))]
        [IsTheDateWhenHairdressingSalonIsOpen(ErrorMessage = "The hairdressing salon is closed that day!")]
        public DateTime Date { get; set; }
    }

    public class VisitInfoBindingModel
    {
        public int Id { get; set; }
        [JsonConverter(typeof(DateWithHourTimeConverter))]
        public DateTime StartDate { get; set; }
        [NotMapped]
        public int Time { get; private set; } = Constants.Constants.VISIT_TIME;
        public decimal Cost { get; set; }
        public decimal CostAfterDiscount { get; set; }
        public ServiceBindingModel Service { get; set; }
        public CustomerInfoBindingModel Customer { get; set; }

    }

    public class UserVisitInfoBindingModel
    {
        public int Id { get; set; }
        [JsonConverter(typeof(DateWithHourTimeConverter))]
        public DateTime StartDate { get; set; }
        [NotMapped]
        public int Time { get; private set; } = Constants.Constants.VISIT_TIME;
        public decimal Cost { get; set; }
        public ServiceBindingModel Service { get; set; }
    }

    public class VisitTermInfoBindingModel
    {
        public int Id { get; set; }
        [Required]
        [JsonConverter(typeof(DateWithHourTimeConverter))]
        public DateTime StartDate { get; set; }
        [Required]
        [JsonConverter(typeof(DateWithHourTimeConverter))]
        public DateTime EndDate { get; set; }
        public ICollection<VisitInfoBindingModel> Visits { get; set; }
    }


    public class GetVisitTermByDateBindingModel
    {
        [Required]
        [IsTheDateFromFuture(ErrorMessage = "Data nieaktualna!")]
        [JsonConverter(typeof(DateWithHourTimeConverter))]
        public DateTime Date { get; set; }
    }

    public class UpdateVisitBindingModel
    {
        [Required]
        public decimal Cost { get; set; }
        [Required]
        public int Id { get; set; }
        [Required]
        public int ServiceId { get; set; }

    }
}