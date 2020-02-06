using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FluentAssertions;
using NSubstitute;
using Microsoft.Owin;
using System.Threading.Tasks;
using System.Web;
using System.IO;
using System.Reflection;
using System.Text;
using System.Collections.Specialized;
using System.Globalization;
using System.Collections.Generic;
using System.Linq;

namespace Kentor.OwinCookieSaver.Tests
{
    [TestClass]
    public class KentorOwinCookieSaverMiddlewareTests
    {
        public OwinContext CreateOwinContext()
        {
            var httpRequest = new HttpRequest("", "http://localhost/", "");
            var stringWriter = new StringWriter();
            var httpResponse = new HttpResponse(stringWriter);
            var httpContext = new HttpContext(httpRequest, httpResponse);
            var httpContextBase = new HttpContextWrapper(httpContext);

            var context = new OwinContext();

            context.Environment[typeof(HttpContextBase).FullName] = httpContextBase;
            context.Environment[typeof(HttpContext).FullName] = httpContext;

            return context;
        }

        class MiddlewareMock : OwinMiddleware
        {
            public const string OwinCookie = "OwinCookie=OwinValue; expires=Wed, 13-Jan-2021 22:23:01 GMT; path=/; secure; HttpOnly; SameSite=Strict";
            public const string MinimalCookie = "MinimalCookie=MinimalValue";

            public MiddlewareMock() : base(null) { }

            public IOwinContext callingContext = null;

            public override Task Invoke(IOwinContext context)
            {
                callingContext = context;

                context.Response.Headers.Add("Set-Cookie", new[]
                {
                    OwinCookie,
                    MinimalCookie
                });

                context.Response.Cookies.Append("ComplexCookie", "ComplexValue", new CookieOptions 
                { 
                    Domain = "example.com",
                    Expires = new DateTime(2079, 12, 27, 14, 30, 00, DateTimeKind.Utc),
                    HttpOnly = true,
                    Secure = true,
                    Path = "/SomePath"
                });

                context.Response.Cookies.Append("SimpleCookie", "SimpleValue");

                return Task.FromResult(0);
            }
        }

        [TestMethod]
        public async Task KentorOwinCookieSaverMiddleware_InvokesNext()
        {
            var next = new MiddlewareMock();
            var context = CreateOwinContext();

            var subject = new KentorOwinCookieSaverMiddleware(next);

            await subject.Invoke(context);

            next.callingContext.Should().Be(context);
        }

        [TestMethod]
        public async Task KentorOwinCookieSaverMiddleware_AddsOwinCookies()
        {
            var context = CreateOwinContext();

            var httpContext = context.Get<HttpContextBase>(typeof(HttpContextBase).FullName);

            httpContext.Response.Cookies.Add(new HttpCookie("SystemWebCookie", "SystemWebValue"));

            var next = new MiddlewareMock();

            var subject = new KentorOwinCookieSaverMiddleware(next);

            await subject.Invoke(context);

            httpContext.Response.Cookies.AllKeys.Should().Contain("OwinCookie");
            httpContext.Response.Cookies.AllKeys.Should().Contain("SystemWebCookie", because: "The original cookie should not be removed");

            var cookie = httpContext.Response.Cookies["OwinCookie"];

            // Time will be parsed to a local time, taking time zone into account. So let's parse the given
            // time and then convert it to local time.
            var expectedExpires = new DateTime(2021, 01, 13, 22, 23, 01, DateTimeKind.Utc).ToLocalTime();

            cookie.Value.Should().Be("OwinValue");
            cookie.Path.Should().Be("/");
            cookie.Expires.Should().Be(expectedExpires);
            cookie.Secure.Should().BeTrue("cookie string contains Secure");
            cookie.HttpOnly.Should().BeTrue("cookie string contains HttpOnly");
            cookie.SameSite.Should().Be(SameSiteMode.Strict, "cookie string contains Strict SameSite setting");
            //cookie.IsFromHeader().Should().BeTrue();
        }

        [TestMethod]
        public async Task KentorOwinCookieSaverMiddleware_ShouldNotOverwriteResponseCookie()
        {
            var context = CreateOwinContext();

            var httpContext = context.Get<HttpContextBase>(typeof(HttpContextBase).FullName);

            httpContext.Response.Cookies.Add(new HttpCookie("OwinCookie", "TheOriginalCookie"));

            var next = new MiddlewareMock();

            var subject = new KentorOwinCookieSaverMiddleware(next);

            await subject.Invoke(context);

            httpContext.Response.Cookies.AllKeys.Should().Contain("OwinCookie");

            var cookie = httpContext.Response.Cookies["OwinCookie"];
            cookie.Should().NotBeNull();
            cookie.Value.Should().Be("TheOriginalCookie");
        }

        [TestMethod]
        public async Task KentorOwinCookieSaverMiddleware_RoundTripsComplexCookie()
        {
            var context = CreateOwinContext();
            var next = new MiddlewareMock();
            var subject = new KentorOwinCookieSaverMiddleware(next);

            await subject.Invoke(context);

            var before = MiddlewareMock.OwinCookie;

            var rebuiltHeader = RegenerateSetCookieHeader(context)
                .Single(s => s.StartsWith("OwinCookie"));

            rebuiltHeader.Should().Be(before);
        }

        [TestMethod]
        public async Task KentorOwinCookieSaverMiddleware_RoundTripsMinimalCookie()
        {
            var context = CreateOwinContext();
            var next = new MiddlewareMock();
            var subject = new KentorOwinCookieSaverMiddleware(next);

            await subject.Invoke(context);

            var before = MiddlewareMock.MinimalCookie;

            var rebuiltHeader = RegenerateSetCookieHeader(context)
                .Single(s => s.StartsWith("MinimalCookie"));

            rebuiltHeader.Should().Be(before);
        }

        [TestMethod]
        public async Task KentorOwinCookieSaverMiddleware_HandlesMissingHttpContext()
        {
            var context = new OwinContext();

            var next = new MiddlewareMock();

            var subject = new KentorOwinCookieSaverMiddleware(next);

            // Should not throw.
            await subject.Invoke(context);
        }

        [TestMethod]
        public async Task KentorOwinCookieSaverMiddleware_AbortsOnHeadersSent()
        {
            var context = CreateOwinContext();
            var next = new MiddlewareMock();
            var subject = new KentorOwinCookieSaverMiddleware(next);

            // The property has an internal setter.
            var headersWrittenProperty = typeof(HttpResponse)
                .GetProperty("HeadersWritten");

            headersWrittenProperty.GetSetMethod(true).Invoke(
                context.Environment[typeof(HttpContext).FullName]
                    .As<HttpContext>().Response,
                new object[] { true });

            await subject.Invoke(context);

            // With headers already written, the middleware should not try
            // to write to the cookie collection.
            context.Environment[typeof(HttpContextBase).FullName]
                .As<HttpContextBase>().Response.Cookies
                .Should().BeEmpty();
        }

        private static IEnumerable<string> RegenerateSetCookieHeader(IOwinContext context)
        {
            var httpContext = context.Get<HttpContextBase>(typeof(HttpContextBase).FullName);

            // Copied and adapted the code that regenerates the Set-Cookie header from 
            // System.Web.GenerateResponseHeadersForCookies and System.Web.HttpCookie.GetSetCookieHeader

            // write all the cookies again
            for (int c = 0; c < httpContext.Response.Cookies.Count; c++)
            {
                // generate a Set-Cookie header for each cookie
                var cookie = httpContext.Response.Cookies[c];

                StringBuilder s = new StringBuilder();

                // cookiename=
                if (!String.IsNullOrEmpty(cookie.Name))
                {
                    s.Append(cookie.Name);
                    s.Append('=');
                }

                // key=value&...
                var _multiValue = (NameValueCollection)typeof(HttpCookie)
                    .GetField("_multiValue", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(cookie);
                if (_multiValue != null)
                {
                    var toString = typeof(HttpContext).Assembly.GetType("HttpValueCollection")
                        .GetMethod("ToString", BindingFlags.NonPublic, null, new Type[] { typeof(bool) }, null);

                    s.Append(toString.Invoke(cookie, new object[] { false }));
                }
                else if (cookie.Value != null)
                    s.Append(cookie.Value);

                // domain
                if (!String.IsNullOrEmpty(cookie.Domain))
                {
                    s.Append("; domain=");
                    s.Append(cookie.Domain);
                }

                // expiration
                if (cookie.Expires != DateTime.MinValue)
                {
                    s.Append("; expires=");
                    var dt = cookie.Expires;
                    if (dt < DateTime.MaxValue.AddDays(-1) && dt > DateTime.MinValue.AddDays(1))
                        dt = dt.ToUniversalTime();
                    s.Append(dt.ToString("ddd, dd-MMM-yyyy HH':'mm':'ss 'GMT'", DateTimeFormatInfo.InvariantInfo));
                }

                // path
                if (!String.IsNullOrEmpty(cookie.Path))
                {
                    s.Append("; path=");
                    s.Append(cookie.Path);
                }

                // secure
                if (cookie.Secure)
                    s.Append("; secure");

                // httponly
                if (cookie.HttpOnly)
                {
                    s.Append("; HttpOnly");
                }

                if (Enum.IsDefined(typeof(SameSiteMode), cookie.SameSite))
                {
                    s.Append("; SameSite=");
                    s.Append(cookie.SameSite.ToString());
                }

                yield return s.ToString();
            }
        }
    }
}
