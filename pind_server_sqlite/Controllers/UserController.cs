using Microsoft.Ajax.Utilities;
using pind_server_sqlite.App_Start;
using pind_server_sqlite.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
namespace pind_server_sqlite.Controllers
{
    [ApiAuthorizationFilter]
    [ApiExceptionFilter]
    public class UserController : ApiController
    {
        [HttpPost]
        [Route("user/logout")]
        public IHttpActionResult Logout()
        {
            Request.Properties.TryGetValue("userid", out Object userid);
            if (userid == null || string.IsNullOrWhiteSpace(userid.ToString()))
            {
                throw new CustomException("参数错误");
            }

            //HttpContext.Current.Response.Cookies.Remove("accesstoken");

            HttpCookie cookie = new HttpCookie("accesstoken", "")
            {
                Expires = DateTime.Now.AddMilliseconds(-1),
                Secure = true,
                HttpOnly = true,
                SameSite = SameSiteMode.Lax
            };

            HttpContext.Current.Response.Cookies.Add(cookie);

            return Json(new { code = 1, data = new { }, message = "ok" });
        }
    }
}
