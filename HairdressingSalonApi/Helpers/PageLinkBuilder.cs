using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http.Routing;

namespace WebApplication3.Helpers
{
    public class PageLinkBuilder
    {
        public Uri FirstPage { get; private set; }
        public Uri LastPage { get; private set; }
        public Uri NextPage { get; private set; }
        public Uri PreviousPage { get; private set; }

        public PageLinkBuilder(UrlHelper urlHelper, string routeName, object routeValues, int pageNo, int pageSize, long totalRecordCount)
        {
            var pageCount = totalRecordCount > 0
                ? Math.Ceiling((double)totalRecordCount / (double)pageSize)
                : 0;

            FirstPage = new Uri(urlHelper.Link(routeName, new HttpRouteValueDictionary(routeValues)
            {
                { "pageNo" , 1 },
                { "pageSize", pageSize }
            }));

            LastPage = new Uri(urlHelper.Link(routeName, new HttpRouteValueDictionary(routeValues)
            {
                {"pageNo", pageCount },
                {"pageSize", pageSize }
            }));

            if (pageNo < pageCount)
            {
                NextPage = new Uri(urlHelper.Link(routeName, new HttpRouteValueDictionary(routeValues)
                {
                    {"pageNo", pageNo + 1 },
                    {"pageSize", pageSize }
                }));
            }

            if (pageNo > 1)
            {
                PreviousPage = new Uri(urlHelper.Link(routeName, new HttpRouteValueDictionary(routeValues)
                {
                    {"pageNo", pageNo - 1 },
                    {"pageSize", pageSize }
                }));
            }
        }
    }
}