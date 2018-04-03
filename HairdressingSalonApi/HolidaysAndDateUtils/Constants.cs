using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;

namespace WebApplication3.Constants
{
    public static class Constants
    {
        //Date Constants
        public static DateTime OPENING_HOUR_OF_SALON = DateTime.ParseExact("08:00", "HH:mm", CultureInfo.InvariantCulture);
        public static DateTime CLOSURE_HOUR_OF_SALON = DateTime.ParseExact("17:00", "HH:mm", CultureInfo.InvariantCulture);
        public static int VISIT_TIME = 30;

        //Settings

        public static DateTime GET_OPENING_HOUR_FROM_DATE(DateTime date)
        {
            return new DateTime(date.Year, date.Month, date.Day, OPENING_HOUR_OF_SALON.Hour, OPENING_HOUR_OF_SALON.Minute, OPENING_HOUR_OF_SALON.Second);
        }
        public static DateTime GET_CLOSURE_HOUR_FROM_DATE(DateTime date)
        {
            return new DateTime(date.Year, date.Month, date.Day, CLOSURE_HOUR_OF_SALON.Hour, CLOSURE_HOUR_OF_SALON.Minute, CLOSURE_HOUR_OF_SALON.Second);
        }


    }
}