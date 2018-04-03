using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Linq.Dynamic;

namespace WebApplication3.Helpers
{
    public static class IQueryableExtensions 
    {
        public static IQueryable<T> ApplySort<T>(this IQueryable<T> source, string sort)
        {
            if(source == null)
            {
                throw new ArgumentNullException();
            }

            if (sort == null)
            {
                return source;
            }

            var lstSort = sort.Split(',');

            string completeSortExpression = "";

            foreach(var sortOption in lstSort)
            {
                if (sortOption.StartsWith("-"))
                {
                    completeSortExpression = completeSortExpression + sortOption.Remove(0, 1) + " descending,";
                }
                else
                {
                    completeSortExpression = completeSortExpression + sortOption + ",";
                }
            }
            if (!string.IsNullOrWhiteSpace(completeSortExpression))
            {
                source = source.OrderBy(completeSortExpression.Remove(completeSortExpression.Count() - 1));
            }
            return source;
        }
    }
}