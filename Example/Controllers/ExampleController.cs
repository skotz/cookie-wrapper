using ExampleFramework.Models;
using Skotz;
using System.Web;
using System.Web.Mvc;

namespace ExampleFramework.Controllers
{
    public class ExampleController : Controller
    {
        private CookieWrapper _cookies;

        public ExampleController()
        {
            _cookies = new CookieWrapper();
        }

        public ActionResult Index()
        {
            var model = new IndexModel();

            // The bad way to update cookies
            UpdateCookieTheWrongWay("badcookie", "A");
            UpdateCookieTheWrongWay("badcookie", "B");
            UpdateCookieTheWrongWay("badcookie", "C");
            UpdateCookieTheWrongWay("badcookie", "D");

            // The good way to update cookies
            UpdateCookie("goodcookie", "A");
            UpdateCookie("goodcookie", "B");
            UpdateCookie("goodcookie", "C");
            UpdateCookie("goodcookie", "D");

            model.BadValue = _cookies["badcookie"].Value;
            model.GoodValue = _cookies["goodcookie"].Value;

            return View(model);
        }

        private void UpdateCookieTheWrongWay(string name, string value)
        {
            // Warning: don't do this!
            var cookie = System.Web.HttpContext.Current.Request.Cookies[name];
            if (cookie == null)
            {
                cookie = new HttpCookie(name);
            }
            cookie.Value = cookie.Value + value;
            System.Web.HttpContext.Current.Response.SetCookie(cookie);
        }

        private void UpdateCookie(string name, string value)
        {
            var cookie = _cookies[name];
            if (cookie == null)
            {
                cookie = new HttpCookie(name);
            }
            cookie.Value = cookie.Value + value;
            _cookies.SetCookie(cookie);
        }
    }
}