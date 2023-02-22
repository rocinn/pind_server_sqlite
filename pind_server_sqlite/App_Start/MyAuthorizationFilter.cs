using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Filters;
using System.Web.Routing;
using pind_server_sqlite.Common;

namespace pind_server_sqlite.App_Start
{
    public class MyAuthorizationFilter : IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationContext filterContext)
        {
            ILog m_log = LogManager.GetLogger("MyAuthorizationFilter");
            string allcookie = JsonConvert.SerializeObject(HttpContext.Current.Request.Cookies);
            m_log.Info($"allcookie:{allcookie}");
            HttpCookie accesstokenCookie = HttpContext.Current.Request.Cookies["accesstoken"];
            string ctrlName = filterContext.ActionDescriptor.ControllerDescriptor.ControllerName;
            string actionName = filterContext.ActionDescriptor.ActionName;
            m_log.Info($"ControllerName:{ctrlName}|ActionName:{actionName}");
            string _userid = "";
            string _username = "";

            //if (ctrlName.ToLower() == "home" && (actionName.ToLower() == "login"))
            //{
            //    //什么都不做
            //}
            //else
            //{
            //if (filterContext.HttpContext.["username"] == null)
            //{
            //    ContentResult contentResult = new ContentResult();
            //    contentResult.Content = "没有登录";
            //    //filterContext.Result = contentResult;
            //    filterContext.Result = new RedirectResult("/Login/Index");
            //}

            if (accesstokenCookie == null || string.IsNullOrWhiteSpace(accesstokenCookie.Value))
            {
                //RedirectToLogin(filterContext);
                //throw new CustomException("need to login 001");
                //return;

                m_log.Info($"accesstokenCookie is empty");
            }
            else
            {
                string access = accesstokenCookie.Value;
                string data = AccessToken.getOriginalData(access);

                m_log.Info($"access origin data: {(data == null ? "" : data)}");
                if (string.IsNullOrWhiteSpace(data))
                {
                    //RedirectToLogin(filterContext);
                    //throw new CustomException("need to login 002");
                    //return;
                }
                else
                {
                    Dictionary<string, object> dicData = JsonConvert.DeserializeObject<Dictionary<string, object>>(data);
                    if (dicData == null || !dicData.ContainsKey("userid") || dicData["userid"] == null || !dicData.ContainsKey("expire") || dicData["expire"] == null)
                    {
                        //RedirectToLogin(filterContext);
                        //throw new CustomException("need to login 003");
                        //return;
                    }
                    else
                    {
                        DateTime.TryParse(dicData["expire"].ToString(), out DateTime dee);
                        if (dee < DateTime.Now)
                        {
                            //RedirectToLogin(filterContext);
                            //throw new CustomException($"need to login 004: {dee.ToString("yyyy-MM-dd HH:mm:ss")}");
                            //return;


                        }
                        else
                        {
                            _userid = dicData["userid"].ToString();
                            _username = dicData["name"].ToString();
                        }
                    }
                }
            }

            HttpContext.Current.Items.Add("userid", _userid);
            HttpContext.Current.Items.Add("name", _username);
            //}
        }

        private void RedirectToLogin(AuthorizationContext filterContext)
        {
            filterContext.Result = new RedirectResult("/Home/Login");
        }
    }
}