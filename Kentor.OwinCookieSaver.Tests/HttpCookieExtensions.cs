using System;
using System.Reflection;
using System.Web;

namespace Kentor.OwinCookieSaver.Tests
{
    static class HttpCookieExtensions
    {
        /// <summary>
        /// The "From Header" Property no longer exists in HttpCookie :-(
        /// </summary>
        [Obsolete("The FromHeader property no longer exists in System.Web.HttpCookie")]
        private static readonly PropertyInfo FromHeaderProperty = typeof(HttpCookie).GetProperty("FromHeader", BindingFlags.NonPublic | BindingFlags.Instance);

        [Obsolete("The FromHeader property no longer exists in System.Web.HttpCookie")]
        public static bool IsFromHeader(this HttpCookie cookie)
        {
            return (bool)FromHeaderProperty.GetValue(cookie);
        }
    }
}
