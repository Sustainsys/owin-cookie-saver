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
        [TestInitialize]
        public void SetHttpContextCurrent()
        {
            var httpRequest = new HttpRequest("", "http://localhost/", "");
            var stringWriter = new StringWriter();
            var httpResponce = new HttpResponse(stringWriter);
            var httpContext = new HttpContext(httpRequest, httpResponce);

            HttpContext.Current = httpContext;
        }

        [TestCleanup]
        public void ClearHttpContextCurrent()
        {
            HttpContext.Current = null;
        }

        class MiddlewareMock : OwinMiddleware
        {
            public MiddlewareMock() : base(null) { }

            public IOwinContext callingContext = null;

            public override Task Invoke(IOwinContext context)
            {
                callingContext = context;

                context.Response.Headers.Add("Set-Cookie", new[]
                {
                    "OwinCookie=OwinValue; expires=Wed, 13-Jan-2021 22:23:01 GMT; path=/; secure; HttpOnly",
                    "MinimalCookie=MinimalValue"
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
            var context = new OwinContext();

            var subject = new KentorOwinCookieSaverMiddleware(next);

            await subject.Invoke(context);

            next.callingContext.Should().Be(context);
        }

        [TestMethod]
        public void KentorOwinCookieSaverMiddleware_AddsOwinCookies()
        {
            HttpContext.Current.Response.Cookies.Add(new HttpCookie("SystemWebCookie", "SystemWebValue"));

            var context = new OwinContext();

            var next = new MiddlewareMock();

            var subject = new KentorOwinCookieSaverMiddleware(next);

            subject.Invoke(context);

            HttpContext.Current.Response.Cookies.AllKeys.Should().Contain("OwinCookie");

            var cookie = HttpContext.Current.Response.Cookies["OwinCookie"];

            // Time will be parsed to a local time, taking time zone into account. So let's parse the given
            // time and then convert it to local time.
            var expectedExpires = new DateTime(2021, 01, 13, 22, 23, 01, DateTimeKind.Utc).ToLocalTime();

            cookie.Value.Should().Be("OwinValue");
            cookie.Path.Should().Be("/");
            cookie.Expires.Should().Be(expectedExpires);
            cookie.Secure.Should().BeTrue("cookie string contains Secure");
            cookie.HttpOnly.Should().BeTrue("cookie string contains HttpOnly");
        }

        [TestMethod]
        public void KentorOwinCookieSaverMiddleware_RoundtripsComplexCookie()
        {
            var context = new OwinContext();
            var next = new MiddlewareMock();
            var subject = new KentorOwinCookieSaverMiddleware(next);

            subject.Invoke(context);

            // The first cookie is 85 chars long.
            var before = context.Response.Headers["Set-Cookie"].Substring(0, 85);

            var rebuiltHeader = RegenerateSetCookieHeader().Single(s => s.StartsWith("OwinCookie"));

            rebuiltHeader.Should().Be(before);
        }

        [TestMethod]
        public void KentorOwinCookieSaverMiddleware_RoundtripsMinimalCookie()
        {
            var context = new OwinContext();
            var next = new MiddlewareMock();
            var subject = new KentorOwinCookieSaverMiddleware(next);

            subject.Invoke(context);

            // The interesting cookie is at offset 86 and is 26 chars long.
            var before = context.Response.Headers["Set-Cookie"].Substring(86, 26);

            var rebuiltHeader = RegenerateSetCookieHeader().Single(s => s.StartsWith("MinimalCookie"));

            rebuiltHeader.Should().Be(before);
        }

        private static IEnumerable<string> RegenerateSetCookieHeader()
        {
            // Copied and adapted the code that regenerates the Set-Cookie header from 
            // System.Web.GenerateResponseHeadersForCookies and System.Web.HttpCookie.GetSetCookieHeader

            // write all the cookies again
            for (int c = 0; c < HttpContext.Current.Response.Cookies.Count; c++)
            {
                // generate a Set-Cookie header for each cookie
                var cookie = HttpContext.Current.Response.Cookies[c];

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

                yield return s.ToString();
            }
        }
    }
}
