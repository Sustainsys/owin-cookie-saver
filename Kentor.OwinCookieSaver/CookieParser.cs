using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Kentor.OwinCookieSaver
{
    static class CookieParser
    {
        public static IEnumerable<HttpCookie> Parse(IList<string> setCookieHeader)
        {
            foreach (var c in setCookieHeader)
            {
                var segments = c.Split(';');

                var nameAndValue = GetKeyValue(segments[0]);

                HttpCookie cookie = new HttpCookie(nameAndValue.Key, nameAndValue.Value)
                {
                    // Path defaults to /, want to be able to roundtrip non-existing field.
                    Path = null
                };

                // First key-value-pair is cookie name and value, now look at the rest.
                for (int i = 1; i < segments.Length; i++)
                {
                    var kv = GetKeyValue(segments[i]);

                    if "Expires".Equals(kv.Key, StringComparison.OrdinalIgnoreCase))
                    {
                        cookie.Expires = DateTime.Parse(kv.Value, CultureInfo.InvariantCulture);
                    }
                    else if ("Secure".Equals(kv.Key, StringComparison.OrdinalIgnoreCase))
                    {
                        cookie.Secure = true;
                    }
                    else if ("HttpOnly".Equals(kv.Key, StringComparison.OrdinalIgnoreCase))
                    {
                        cookie.HttpOnly = true;
                    }
                    else if ("Path".Equals(kv.Key, StringComparison.OrdinalIgnoreCase))
                    {
                        cookie.Path = kv.Value;
                    }
                    else if ("Domain".Equals(kv.Key, StringComparison.OrdinalIgnoreCase))
                    {
                        cookie.Domain = kv.Value;
                    }
                    else if ("SameSite".Equals(kv.Key, StringComparison.OrdinalIgnoreCase) &&
                             Enum.TryParse(kv.Value, var out enumValue))
                        cookie.SameSite = (SameSiteMode)enumValue;
                    }                                 
                }

                yield return cookie;
            }
        }

        public static KeyValuePair<string, string> GetKeyValue(string segment)
        {
            var separatorIndex = segment.IndexOf('=');

            if (separatorIndex == -1)
            {
                return new KeyValuePair<string, string>(segment.Trim(), null);
            }
            else
            {
                return new KeyValuePair<string, string>(
                    segment.Substring(0, separatorIndex).Trim(),
                    segment.Substring(separatorIndex + 1, segment.Length - separatorIndex - 1));
            }
        }
    }
}
