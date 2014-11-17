using Microsoft.Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kentor.OwinCookieSaver
{
    public class KentorOwinCookieSaverMiddleware: OwinMiddleware
    {
        public KentorOwinCookieSaverMiddleware(OwinMiddleware next)
            : base(next) { }

        public async override Task Invoke(IOwinContext context)
        {
            await Next.Invoke(context);
        }
    }
}
