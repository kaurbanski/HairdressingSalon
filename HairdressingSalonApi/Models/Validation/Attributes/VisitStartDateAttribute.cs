using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using WebApplication3.Constants;

namespace WebApplication3.Models.Validation.Attributes
{
    public class VisitStartDateAttribute : ValidationAttribute
    {
        private DateTime StartHour = Constants.Constants.OPENING_HOUR_OF_SALON;
        private DateTime EndHour = Constants.Constants.CLOSURE_HOUR_OF_SALON;

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var date = (DateTime)value;
            if (date.TimeOfDay >= EndHour.TimeOfDay ||
                date.TimeOfDay < StartHour.TimeOfDay ||
                DateUtils.IsHoliday(date))
            {
                return new ValidationResult(ErrorMessageString);
            }

            else if (!(date.Minute.ToString() == "0" ||
              date.Minute.ToString() == "30"))
            {
                return new ValidationResult(ErrorMessageString);
            }
            else
            {
                return ValidationResult.Success;
            }
        }
    }

}