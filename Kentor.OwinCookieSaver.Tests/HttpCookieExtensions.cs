using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Kentor.OwinCookieSaver.Tests
{
    static class HttpCookieExtensions
    {
        static PropertyInfo fromHeaderProperty = typeof(HttpCookie).GetProperty(
            "FromHeader", BindingFlags.NonPublic | BindingFlags.Instance);

        public static bool IsFromHeader(this HttpCookie cookie)
        {
            return (bool)fromHeaderProperty.GetValue(cookie);
        }
    }
}
