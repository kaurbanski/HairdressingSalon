using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WebApplication3.Models.Validation.Attributes
{
    public class IsTheDateFromFuture : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if ((DateTime)value < DateTime.UtcNow)
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