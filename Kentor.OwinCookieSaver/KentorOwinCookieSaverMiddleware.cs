using Microsoft.Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Kentor.OwinCookieSaver
{
    /// <summary>
    /// Cookie saving middleware.
    /// </summary>
    public class KentorOwinCookieSaverMiddleware: OwinMiddleware
    {
        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="next">Next middleware in chain</param>
        public KentorOwinCookieSaverMiddleware(OwinMiddleware next)
            : base(next) { }

        /// <summary>
        /// Main entry point of middleware.
        /// </summary>
        /// <param name="context">Owin Context</param>
        /// <returns>Task</returns>
        public async override Task Invoke(IOwinContext context)
        {
            await Next.Invoke(context);

            var setCookie = context.Response.Headers.GetValues("Set-Cookie");
            if(setCookie != null)
            {
                var cookies = CookieParser.Parse(setCookie);
                
                foreach(var c in cookies)
                {
                    if(!HttpContext.Current.Response.Cookies.AllKeys.Contains(c.Name))
                    {
                        HttpContext.Current.Response.Cookies.Add(c);
                    }
                }
            }
        }
    }
}
