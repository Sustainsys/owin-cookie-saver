using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SampleWebApp.Controllers
{
    public class ReproductionController : Controller
    {
        public ActionResult Index()
        {
            // First make sure sessions have been used in the app.
            Session["StartSession"] = "SomeValue";

            return RedirectToAction("ClearCookies");
        }

        public ActionResult ClearCookies()
        {
            // Then abandon the session and clear all cookies to start clean.
            Session.Abandon();

            foreach (var cookie in Request.Cookies.Cast<string>().ToList())
            {
                Response.Cookies.Set(new HttpCookie(cookie)
                    {
                        Expires = DateTime.Now.AddYears(-1)
                    });
            }

            return RedirectToAction("SetAuthCookie");
        }

        // Deliberately no SetAuthCookie Action - it is intercepted by middleware.

        public ActionResult Results()
        {
            return View();
        }
    }
}