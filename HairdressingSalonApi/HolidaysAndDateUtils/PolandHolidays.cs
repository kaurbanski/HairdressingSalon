using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplication3.Constants
{
    public class PolandHolidays : CatholicBaseProvider
    {
        public  static IEnumerable<Holiday> GetHolidays(int year)
        {
            var easterSunday = EasterSunday(year);

            var items = new List<Holiday>();
            items.Add(new Holiday(year, 1, 1, "Nowy Rok", "New Year's Day"));
            items.Add(new Holiday(year, 1, 6, "Święto Trzech Króli", "Epiphany"));
            items.Add(new Holiday(easterSunday, "Pierwszy dzień Wielkiej Nocy", "Easter Day"));
            items.Add(new Holiday(easterSunday.AddDays(1), "Drugi dzień Wielkiej Nocy", "Easter Monday"));
            items.Add(new Holiday(year, 5, 1, "Święto Państwowe", "May Day"));
            items.Add(new Holiday(year, 5, 3, "Święto Narodowe Trzeciego Maja", "Constitution Day"));
            items.Add(new Holiday(easterSunday.AddDays(49), "Pierwszy dzień Zielonych Świątek", "Pentecost Sunday"));
            items.Add(new Holiday(easterSunday.AddDays(60), "Dzień Bożego Ciała", "Corpus Christi"));
            items.Add(new Holiday(year, 8, 15, "Wniebowzięcie Najświętszej Maryi Panny", "Assumption Day"));
            items.Add(new Holiday(year, 11, 1, "Wszystkich Świętych", "All Saints' Day"));
            items.Add(new Holiday(year, 11, 11, "Narodowe Święto Niepodległości", "Independence Day"));
            items.Add(new Holiday(year, 12, 25, "Pierwszy dzień Bożego Narodzenia", "Christmas Day"));
            items.Add(new Holiday(year, 12, 26, "Drugi dzień Bożego Narodzenia", "St. Stephen's Day"));

            return items.OrderBy(x => x.Date);
        }
    }
}