#Kentor.OwinCookieSaver

There is a bug in Microsoft's Owin implementation for System.Web. The one that
is being used when running Owin applications on IIS. Which is what probably
99% of us do, if we're using the new Owin-based authentication handling with
ASP.NET MVC5.

The [bug](https://katanaproject.codeplex.com/workitem/197) makes cookies set
by Owin mysteriously disappear on some occasions.

This middleware is a fix for that bug. Simple add it *before* the any cookie
handling middleware and it will preserve the authentication cookies.

    app.UseKentorOwinCookieSaver();
    
    app.UseCookieAuthentication(new CookieAuthenticationOptions());

##Nuget package
The middleware is available as a nuget package, 
[Kentor.OwinCookieSaver](https://www.nuget.org/packages/Kentor.OwinCookieSaver/).

##Limitations
Due to have the cookies are handled, this middleware is only able to save
cookies that are added by middleware; such as the authentication cookie. It won't
save cookies handled by normal MVC Actions. In that case, the workaround
of adding a dummy session value is recommended (if the application uses session,
which is the most likely reason to be affected by this bug).

##Plans for a Real Fix
We're looking into contributing the fix directly to Katana to make this
middleware redundant. That would also make the fix work for all cookies
and not only those added by authentication middleware.
