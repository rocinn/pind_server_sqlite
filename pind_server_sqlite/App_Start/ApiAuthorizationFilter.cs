using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using pind_server_sqlite.Common;

namespace pind_server_sqlite.App_Start
{
    public class ApiAuthorizationFilter: AuthorizationFilterAttribute
    {
        public override void OnAuthorization(HttpActionContext actionContext)
        {
            base.OnAuthorization(actionContext);

            HttpCookie accesstokenCookie = HttpContext.Current.Request.Cookies["accesstoken"];
            if (accesstokenCookie == null || string.IsNullOrWhiteSpace(accesstokenCookie.Value))
            {
                throw new CustomException("need to login 001");
            }

            string access = accesstokenCookie.Value;
            string data = AccessToken.getOriginalData(access);
            if (string.IsNullOrWhiteSpace(data))
            {
                throw new CustomException("need to login 002");
            }

            ILog m_log = LogManager.GetLogger("ApiAuthorizationFilter");
            m_log.Info($"access origin data: {(data == null ? "" : data)}");

            Dictionary<string, object> dicData = JsonConvert.DeserializeObject<Dictionary<string, object>>(data);
            if (dicData == null || !dicData.ContainsKey("userid") || dicData["userid"] == null || !dicData.ContainsKey("expire") || dicData["expire"] == null)
            {
                throw new CustomException("need to login 003");
            }

            DateTime.TryParse(dicData["expire"].ToString(), out DateTime dee);
            if (dee < DateTime.Now)
                throw new CustomException($"need to login 004: {dee.ToString("yyyy-MM-dd HH:mm:ss")}");

            actionContext.Request.Properties.Add("userid", dicData["userid"].ToString());
            actionContext.Request.Properties.Add("name", dicData["name"]?.ToString());
        }
    }
}