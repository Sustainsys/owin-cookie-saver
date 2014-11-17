using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using FluentAssertions;
using Owin;
using Microsoft.Owin;

namespace Kentor.OwinCookieSaver.Tests
{
    [TestClass]
    public class KentorOwinCookieSaverExtensionsTests
    {
        [TestMethod]
        public void KentorOwinCookieSaverExtensions_Use_RegistersMiddleware()
        {
            var app = Substitute.For<IAppBuilder>();

            app.UseKentorOwinCookieSaver();

            app.Received().Use(typeof(KentorOwinCookieSaverMiddleware));
        }
    }
}
