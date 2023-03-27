using System.Web.Mvc;

namespace pind_server_sqlite.Controllers
{
    public class HomeController : Controller
    {
        // GET: Home
        public ActionResult Index()
        {
            object userid = HttpContext.Items["userid"];
            if (userid == null || string.IsNullOrWhiteSpace(userid.ToString()))
            {
                return Redirect("Login");
            }

            return View();
        }
    }
}