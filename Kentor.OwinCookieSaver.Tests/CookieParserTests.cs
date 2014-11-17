using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FluentAssertions;
using System.Linq;
using System.Collections.Generic;
using System.Web;

namespace Kentor.OwinCookieSaver.Tests
{
    [TestClass]
    public class CookieParserTests
    {
        [TestMethod]
        public void CookieParser_NameAndValueOnly()
        {
            var cookieHeader = new List<string>() { "Name=Value" };

            var result = CookieParser.Parse(cookieHeader).Single();

            var expected = new HttpCookie("Name", "Value");

            result.ShouldBeEquivalentTo(expected);
        }

        [TestMethod]
        public void CookieParser_AllFields()
        {
            var cookieHeader = new List<string>()
            {
                "Name=Value; Domain=example.com; Expires=Wed, 13 Jan 2021, 22:23:01 GMT; HttpOnly; Secure; Path=/SomePath"
            };

            var result = CookieParser.Parse(cookieHeader).Single();

            var expected = new HttpCookie("Name", "Value")
            {
                Domain = "example.com",
                Expires = new DateTime(2021, 01, 13, 22, 23, 01, DateTimeKind.Utc).ToLocalTime(),
                HttpOnly = true,
                Path = "/SomePath",
                Secure = true
            };

            result.ShouldBeEquivalentTo(expected);
        }
    }
}
