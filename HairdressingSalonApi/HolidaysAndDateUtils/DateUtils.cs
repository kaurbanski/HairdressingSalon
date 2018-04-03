using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebApplication3.Models;
using WebApplication3.Models.ApiBindingModels;

namespace WebApplication3.Constants
{
    public class DateUtils
    {
        public static bool IsHoliday(DateTime value)
        {
            var holidays = PolandHolidays.GetHolidays(value.Year);

            return (value.DayOfWeek == DayOfWeek.Sunday) ||
                (holidays.Any(holiday => holiday.Date.Day == value.Day &&
                holiday.Date.Month == value.Month));
        }

        public static DateTime GetTodayDateWithNoMinutesAndHours()
        {
            DateTime now = DateTime.Now;
            DateTime today = DateTime.Now.Date;
            return today;
        }

        public static List<GetVisitTermByDateBindingModel> GetFreeHoursFromVisits(ICollection<Visit> visits, DateTime date)
        {
            DateTime start = Constants.GET_OPENING_HOUR_FROM_DATE(date);
            DateTime end = Constants.GET_CLOSURE_HOUR_FROM_DATE(date);
            DateTime current;
            List<GetVisitTermByDateBindingModel> hoursList = new List<GetVisitTermByDateBindingModel>();

            for (current = start;
                current >= start &&
                current < end;
                current = current.AddMinutes(Constants.VISIT_TIME))
            {

                if (!visits.Any(x => x.StartDate == current))
                {
                    GetVisitTermByDateBindingModel model = new GetVisitTermByDateBindingModel();
                    model.Date = current;
                    hoursList.Add(model);
                }
            }
            return hoursList;
        }
    }
}