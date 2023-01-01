using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http.Filters;

namespace pind_server_sqlite.App_Start
{
    public class ExceptionFilter : ExceptionFilterAttribute
    {
        public override void OnException(HttpActionExecutedContext actionExecutedContext)
        {
            ILog m_log = LogManager.GetLogger("OnException");
            m_log.Error(actionExecutedContext.Exception.ToString());
        }
    }
}