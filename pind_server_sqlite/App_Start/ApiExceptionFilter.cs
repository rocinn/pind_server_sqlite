using log4net;
using Newtonsoft.Json;
using pind_server_sqlite.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Web;
using System.Web.Http.Filters;
using System.Web.UI.WebControls;

namespace pind_server_sqlite.App_Start
{
    public class ApiExceptionFilter : ExceptionFilterAttribute
    {
        public override void OnException(HttpActionExecutedContext actionExecutedContext)
        {
            ILog m_log = LogManager.GetLogger("OnException");
            m_log.Error(actionExecutedContext.Exception.ToString());
            string msg = actionExecutedContext.Exception.Message;
            int code = -1;
            if (actionExecutedContext.Exception.GetType() == typeof(CustomException))
            {

            }
            else if (actionExecutedContext.Exception.GetType() == typeof(LoginException))
            {
                code = 10000;

            }
            else
            {
                msg = "ERROR";
            }

            var data = new
            {
                code = code,
                message = msg
            };

            var resp = actionExecutedContext.Response ?? new System.Net.Http.HttpResponseMessage();
            resp.StatusCode = System.Net.HttpStatusCode.OK;
            resp.Content = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");
            //if(code == 10000)
            //{
            //    resp.Headers.Add("redirect","/login");
            //}

            actionExecutedContext.Exception = null;
            actionExecutedContext.Response = resp;
        }
    }
}