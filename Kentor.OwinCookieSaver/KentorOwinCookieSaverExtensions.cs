using Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kentor.OwinCookieSaver
{
    public static class KentorOwinCookieSaverExtensions
    {
        public static IAppBuilder UseKentorOwinCookieSaver(this IAppBuilder app)
        {
            app.Use(typeof(KentorOwinCookieSaverMiddleware));

            return app;
        }
    }
}
