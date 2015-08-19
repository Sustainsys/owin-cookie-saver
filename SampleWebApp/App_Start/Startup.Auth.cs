using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Owin;
using Owin;
using Microsoft.Owin.Security.Cookies;
using System.Security.Claims;
using System.Threading.Tasks;
using Kentor.OwinCookieSaver;

[assembly: OwinStartup(typeof(SampleWebApp.App_Start.Startup))]

namespace SampleWebApp.App_Start
{
    public class Startup
    {
        class ConditionalMiddlewareInvoker : OwinMiddleware
        {
            public ConditionalMiddlewareInvoker(OwinMiddleware next)
                : base(next) { }

            public async override Task Invoke(IOwinContext context)
            {
                if (context.Request.Path.StartsWithSegments(new PathString("/Fixed")) ||
                    context.Request.Path.StartsWithSegments(new PathString("/DuplicateSetCookieHeader")))
                {
                    await (new KentorOwinCookieSaverMiddleware(Next)).Invoke(context);
                }
                else
                {
                    await Next.Invoke(context);
                }
            }
        }

        public void Configuration(IAppBuilder app)
        {
            // In normal applications, the cookie saver middleware would be
            // registered here. In this proof of concept application however
            // we want to be able to show the behaviour both with and without
            // the cookie saver middleware enabled. Because of this a hack is
            // used that only enables the cookie saver middleware when invoked
            // on certain paths.
            app.Use(typeof(ConditionalMiddlewareInvoker));

            app.UseCookieAuthentication(new CookieAuthenticationOptions());

            // Simulate an external login middleware for any Url ending with SetAuthCookie
            app.Use(async (context, next) =>
                {
                    var lastPart = context.Request.Path.Value.Split('/').Last();
                    if (lastPart == "SetAuthCookie")
                    {
                        var identity = new ClaimsIdentity("Cookies");
                        identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, "NameId"));
                        context.Authentication.SignIn(identity);

                        context.Response.Redirect("Results");
                    }
                    else
                    {
                        await next.Invoke();
                    }
                });
        }
    }
}