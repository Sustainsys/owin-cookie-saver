using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FluentAssertions;
using NSubstitute;
using Microsoft.Owin;
using System.Threading.Tasks;
using System.Web;
using System.IO;

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

            //var sessionContainer = new HttpSessionStateContainer("id", new SessionStateItemCollection(),
            //                                        new HttpStaticObjectsCollection(), 10, true,
            //                                        HttpCookieMode.AutoDetect,
            //                                        SessionStateMode.InProc, false);

            //httpContext.Items["AspSession"] = typeof(HttpSessionState).GetConstructor(
            //                            BindingFlags.NonPublic | BindingFlags.Instance,
            //                            null, CallingConventions.Standard,
            //                            new[] { typeof(HttpSessionStateContainer) },
            //                            null)
            //                    .Invoke(new object[] { sessionContainer });

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
                    "OwinCookie=OwinValue; Path=/; Expires=Wed, 13 Jan 2021 22:23:01 GMT; Secure; HttpOnly"
                });
                return Task.FromResult(0);
            }
        }

        [TestMethod]
        public async Task KentorOwinCookieSaverMiddleware_InvokesNext()
        {
            var next = new MiddlewareMock();
            var context = Substitute.For<IOwinContext>();

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
    }
}
