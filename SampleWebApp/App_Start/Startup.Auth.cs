using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Owin;
using Owin;
using Microsoft.Owin.Security.Cookies;
using System.Security.Claims;

[assembly: OwinStartup(typeof(SampleWebApp.App_Start.Startup))]

namespace SampleWebApp.App_Start
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.Use(async (context, next) =>
                {
                    if (context.Request.Path.StartsWithSegments(new PathString("/Fixed")))
                    {
                    }
                    await next.Invoke();
                });

            app.UseCookieAuthentication(new CookieAuthenticationOptions());

            // Simulate an external login middleware for any Url ending with SetAuthCookie
            app.Use(async (context, next) =>
                {
                    var lastPart = context.Request.Path.Value.Split('/').Last();
                    if(lastPart == "SetAuthCookie")
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