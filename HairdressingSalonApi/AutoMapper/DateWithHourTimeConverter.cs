using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplication3.AutoMapper
{
    class DateWithHourTimeConverter : IsoDateTimeConverter
    {
        public DateWithHourTimeConverter()
        {
            base.DateTimeFormat = "dd-MM-yyyy HH:mm";
        }
    }
}