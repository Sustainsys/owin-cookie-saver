# This library is a Legacy Solution
When this library was created, back in 2014, this was a viable solution to the Owin
Cookie problem. It no longer is. Please do not use it, instead follow the
[Microsoft Recommendation](https://github.com/aspnet/AspNetKatana/wiki/System.Web-response-cookie-integration-issues)
to use a SystemWebCookieManager.

# Historical Contents of Readme
*This content is only kept for reference, the information in the following
sections is no longer accurate.*

## Kentor.OwinCookieSaver

There is a bug in Microsoft's Owin implementation for System.Web. The one that
is being used when running Owin applications on IIS. Which is what probably
99% of us do, if we're using the Owin-based authentication handling with
ASP.NET MVC5.

The makes cookies set by Owin mysteriously disappear on some occasions. There
is some [documentation](https://github.com/aspnet/AspNetKatana/wiki/System.Web-response-cookie-integration-issues)
from Microsoft on workarounds, but this middleware offers another solution.

This middleware is a fix for that bug. Simple add it *before* any cookie
handling middleware and it will preserve the authentication cookies.

    app.UseKentorOwinCookieSaver();
    
    app.UseCookieAuthentication(new CookieAuthenticationOptions());

In some cases, a pipeline stage specification might be needed. Using SignalR 
without other middleware containing a stage marker is such a case.

    app.UseKentorOwinCookieSaver(PipelineStage.Authenticate);

## Nuget package
The middleware is available as a nuget package, 
[Kentor.OwinCookieSaver](https://www.nuget.org/packages/Kentor.OwinCookieSaver/).

## Limitations
Due to how the cookies are handled, this middleware is only able to save
cookies that are added by middleware; such as the authentication cookie. It won't
save cookies handled by normal MVC Actions. In that case, the workaround
of adding a dummy session value is recommended (if the application uses session,
which is the most likely reason to be affected by this bug).

## Plans for a Real Fix
I've discussed contributing the functionality of this library directly to
Katana to make this middleware redundant. However Microsoft do not want
this behaviour added as it can affect cookie behaviour. In some cases it will
loose newer flags on cookies. As ASP.NET Core is the clearly the route forward,
there will be no more substantial development on this library.
