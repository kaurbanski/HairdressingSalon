using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplication3.Exceptions
{
    public class VisitCurrentlyReservedException : Exception
    {
        public VisitCurrentlyReservedException() : base()
        {

        }

        public VisitCurrentlyReservedException(String message) : base(message)
        {

        }
    }
}