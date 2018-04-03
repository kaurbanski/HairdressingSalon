using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplication3.Constants
{
    public class CostUtils
    {
        public static decimal GetCostOfTheVisitAfterDiscount(decimal cost, decimal discount)
        {
            return Math.Round(((decimal)((100 - discount)) / 100) * cost, 0);
        }
    }
}