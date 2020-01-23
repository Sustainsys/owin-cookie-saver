using System.Reflection;
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
