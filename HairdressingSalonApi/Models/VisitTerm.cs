using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using WebApplication3.Models.Validation.Attributes;

namespace WebApplication3.Models
{
    public class VisitTerm : IValidatableObject
    {
        private VisitTerm()
        {
            Visits = new List<Visit>();
        }
        [Required]
        public int Id { get; set; }
        [Required]
        [IsTheDateFromFuture(ErrorMessage = "Date must be from future!")]
        public DateTime StartDate { get; private set; }
        [Required]
        [IsTheDateFromFuture(ErrorMessage = "Date must be from future!")]
        public DateTime EndDate { get; private set; }
        public virtual ICollection<Visit> Visits { get; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (StartDate > EndDate)
            {
                yield return new ValidationResult("The start date can not be later than the end date!", new[] { "StartDate" });
            }
        }

        public static VisitTerm Create(DateTime date)
        {
            return new VisitTerm
            {
                StartDate = new DateTime(date.Year, date.Month, date.Day, Constants.Constants.OPENING_HOUR_OF_SALON.Hour, Constants.Constants.OPENING_HOUR_OF_SALON.Minute, Constants.Constants.OPENING_HOUR_OF_SALON.Second),
                EndDate = new DateTime(date.Year, date.Month, date.Day, Constants.Constants.CLOSURE_HOUR_OF_SALON.Hour, Constants.Constants.CLOSURE_HOUR_OF_SALON.Minute, Constants.Constants.CLOSURE_HOUR_OF_SALON.Second)
            };
        }

        public static VisitTerm Create(DateTime date, int id)
        {
            return new VisitTerm
            {
                StartDate = new DateTime(date.Year, date.Month, date.Day, Constants.Constants.OPENING_HOUR_OF_SALON.Hour, Constants.Constants.OPENING_HOUR_OF_SALON.Minute, Constants.Constants.OPENING_HOUR_OF_SALON.Second),
                EndDate = new DateTime(date.Year, date.Month, date.Day, Constants.Constants.CLOSURE_HOUR_OF_SALON.Hour, Constants.Constants.CLOSURE_HOUR_OF_SALON.Minute, Constants.Constants.CLOSURE_HOUR_OF_SALON.Second),
                Id = id
            };
        }
    }
}