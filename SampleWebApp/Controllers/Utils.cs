using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SampleWebApp.Controllers
{
    public static class Utils
    {
        public static void ClearAllCookies(this HttpContextBase httpContext)
        {
            httpContext.Session.Abandon();

            foreach (var cookie in httpContext.Request.Cookies.Cast<string>().ToList())
            {
                httpContext.Response.Cookies.Set(new HttpCookie(cookie)
                {
                    Expires = DateTime.Now.AddYears(-1)
                });
            }

        }
    }
}