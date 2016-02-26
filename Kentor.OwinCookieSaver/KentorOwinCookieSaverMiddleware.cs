using Microsoft.Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Kentor.OwinCookieSaver
{
    /// <summary>
    /// Cookie saving middleware.
    /// </summary>
    public class KentorOwinCookieSaverMiddleware : OwinMiddleware
    {
        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="next">Next middleware in chain</param>
        public KentorOwinCookieSaverMiddleware(OwinMiddleware next)
            : base(next) { }

        static PropertyInfo fromHeaderProperty = typeof(HttpCookie).GetProperty(
            "FromHeader", BindingFlags.NonPublic | BindingFlags.Instance);

        /// <summary>
        /// Main entry point of middleware.
        /// </summary>
        /// <param name="context">Owin Context</param>
        /// <returns>Task</returns>
        public async override Task Invoke(IOwinContext context)
        {
            await Next.Invoke(context);

            var setCookie = context.Response.Headers.GetValues("Set-Cookie");
            if (setCookie != null)
            {
                var cookies = CookieParser.Parse(setCookie);

                foreach (var c in cookies)
                {
                    // Guard for stability, preventing nullref exception
                    // in case private reflection breaks.
                    if (fromHeaderProperty != null)
                    {
                        // Mark the cookie as coming from a header, to prevent
                        // System.Web from adding it again.
                        fromHeaderProperty.SetValue(c, true);
                    }

                    // HttpContext.Current turns to null in some cases after the
                    // async call to Next.Invoke() when run in a web forms
                    // application. Let's grab it from the owin environment
                    // instead.
                    var httpContext = context.Get<HttpContextBase>(typeof(HttpContextBase).FullName);

                    if (httpContext != null
                        && !httpContext.Response.HeadersWritten
                        && !httpContext.Response.Cookies.AllKeys.Contains(c.Name))
                    {
                        httpContext.Response.Cookies.Add(c);
                    }
                }
            }
        }
    }
}
