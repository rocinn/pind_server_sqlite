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
            if (actionExecutedContext.Exception.GetType() == typeof(CustomException))
            {

            }
            else
            {
                msg = "ERROR";
            }

            var data = new
            {
                code = -1,
                message = msg
            };

            var resp = actionExecutedContext.Response ?? new System.Net.Http.HttpResponseMessage();
            resp.StatusCode = System.Net.HttpStatusCode.OK;
            resp.Content = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");

            actionExecutedContext.Exception = null;
            actionExecutedContext.Response = resp;
        }
    }
}