using Kentor.OwinCookieSaver;
using Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Owin
{
    /// <summary>
    /// Extension method for registering middleware.
    /// </summary>
    public static class KentorOwinCookieSaverExtensions
    {
        /// <summary>
        /// Enables the Kentor Owin Cookie Saver middleware, that prevents owin
        /// auth cookies from disappearing. Add above any cookie middleware.
        /// </summary>
        /// <param name="app">Owin app</param>
        /// <returns>Owin app</returns>
        public static IAppBuilder UseKentorOwinCookieSaver(this IAppBuilder app)
        {
            app.Use(typeof(KentorOwinCookieSaverMiddleware));

            return app;
        }
    }
}
