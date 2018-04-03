using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplication3.Exceptions
{
    public class ItemNotFoundException : Exception
    {
        public ItemNotFoundException() : base()
        {

        }

        public ItemNotFoundException(String message) : base(message)
        {

        }
    }
}