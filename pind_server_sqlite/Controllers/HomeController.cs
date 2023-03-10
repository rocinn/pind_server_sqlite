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

            return View();
        }

        public ActionResult Login([Bind(Include = "Username,Password")] Login lg)
        {
            object userid = HttpContext.Items["userid"];
            if (userid == null || string.IsNullOrWhiteSpace(userid.ToString()))
            {
                if (string.IsNullOrWhiteSpace(lg.Username) || string.IsNullOrWhiteSpace(lg.Password))
                {
                    //throw new CustomException("参数错误");
                    return View();
                }
            }
            else
            {
                return RedirectToAction("Index");
            }

            try
            {
                string acctoken = SqliteHelper.GetInstance().fnLogin(lg.Username, lg.Password);
                HttpCookie cookie = new HttpCookie("accesstoken", acctoken)
                {
                    Expires = DateTime.Now.AddDays(1),
                    Secure = true,
                    HttpOnly = true,
                    SameSite = SameSiteMode.Lax
                };

                Response.Cookies.Add(cookie);

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                //ViewData["errorMsg"] = ex.Message;
                return View();
            }
        }
    }
}