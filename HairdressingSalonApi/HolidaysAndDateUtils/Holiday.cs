using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplication3.Constants
{
    public class Holiday
    {
        public DateTime Date { get; set; }
        public string Name { get; set; }
        public string LocalName { get; set; }

        public Holiday(int year, int month, int day, string localName, string name)
        {
            Date = new DateTime(year, month, day);
            Name = name;
            localName = LocalName;
        }

        public Holiday(DateTime date, string localName, string name)
        {
            Date = date;
            Name = name;
            LocalName = localName;
        }
    }
}