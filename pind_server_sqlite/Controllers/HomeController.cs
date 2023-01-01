using Microsoft.Ajax.Utilities;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Http.Filters;
using System.Web.Mvc;
using System.Xml.Linq;
using pind_server_sqlite.App_Start;
using pind_server_sqlite.Common;
using pind_server_sqlite.Models;

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
                return RedirectToAction("Login");
            }

            DataTable dt = SqliteHelper.GetInstance().fnGetNote(1);
            ViewBag.dtNotes = dt;

            return View();
        }

        //[HttpPost]
        public ActionResult Login([Bind(Include = "Username,Password")] Login lg)
        {
            if (string.IsNullOrWhiteSpace(lg.Username) || string.IsNullOrWhiteSpace(lg.Password))
            {
                //throw new CustomException("参数错误");
                return View();
            }

            try
            {
                string acctoken = SqliteHelper.GetInstance().fnLogin(lg.Username, lg.Password);
                HttpCookie cookie = new HttpCookie("accesstoken", acctoken)
                {
                    Expires = DateTime.Now.AddDays(1),
                    Secure = true,
                    HttpOnly = true,
                    SameSite = SameSiteMode.Strict
                };

                Response.Cookies.Add(cookie);

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                return View();
            }
        }
    }
}