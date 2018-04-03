using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using WebApplication3.Constants;

namespace WebApplication3.Models.Validation.Attributes
{
    public class IsTheDateWhenHairdressingSalonIsOpen : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            DateTime date = (DateTime)value;

            if (DateUtils.IsHoliday(date))
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