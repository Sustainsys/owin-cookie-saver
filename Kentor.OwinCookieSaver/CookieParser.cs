using System;
using System.Collections.Generic;
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
            foreach(var c in setCookieHeader)
            {
                var segments = c.Split(';');

                var nameAndValue = GetKeyValue(segments[0]);

                HttpCookie cookie = new HttpCookie(nameAndValue.Key, nameAndValue.Value);

                // First key-value-pair is cookie name and value, now look at the rest.
                for (int i = 1; i < segments.Length; i++)
                {
                    var kv = GetKeyValue(segments[i]);

                    switch(kv.Key)
                    {
                        case "Expires":
                            cookie.Expires = DateTime.Parse(kv.Value);
                            break;
                        case "Secure":
                            cookie.Secure = true;
                            break;
                        case "HttpOnly":
                            cookie.HttpOnly = true;
                            break;
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
