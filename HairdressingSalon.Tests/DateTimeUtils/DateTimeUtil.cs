using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebApplication3.Tests.DateTimeUtils
{
    public static class DateTimeUtil
    {
        public static DateTime GetNextWeekday(DateTime start, DayOfWeek day)
        {
            int daysToAdd = ((int)day - (int)start.DayOfWeek + 7) & 7;
            return start.AddDays(daysToAdd);
        }
    }
}
