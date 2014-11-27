using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace SampleWebApp.Controllers
{
    /// <summary>
    /// Controller for testing  
    /// </summary>
    public class DuplicateSetCookieHeaderController : Controller
    {
        public ActionResult Index()
        {
            // Abandon the session and clear all cookies to start clean.
            HttpContext.ClearAllCookies();

            return RedirectToAction("Results");
        }

        public async Task<ActionResult> Results()
        {
            var handler = new HttpClientHandler
            {
                AllowAutoRedirect = false
            };
            using(var client = new HttpClient(handler))
            {
                var response = await client.GetAsync("http://localhost:51743/DuplicateSetCookieHeader/SetAuthCookie");

                return View(response.Headers.Single(h => h.Key == "Set-Cookie").Value);
            }
        }
    }
}