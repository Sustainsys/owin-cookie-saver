using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FluentAssertions;
using NSubstitute;
using Microsoft.Owin;
using System.Threading.Tasks;

namespace Kentor.OwinCookieSaver.Tests
{
    [TestClass]
    public class KentorOwinCookieSaverMiddlewareTests
    {
        class MiddlewareMock : OwinMiddleware
        {
            public MiddlewareMock() : base(null) { }

            public IOwinContext callingContext = null;

            public override Task Invoke(IOwinContext context)
            {
                callingContext = context;
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
    }
}
