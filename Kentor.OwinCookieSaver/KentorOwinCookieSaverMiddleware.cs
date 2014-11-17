using Microsoft.Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Kentor.OwinCookieSaver
{
    public class KentorOwinCookieSaverMiddleware: OwinMiddleware
    {
        public KentorOwinCookieSaverMiddleware(OwinMiddleware next)
            : base(next) { }

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
