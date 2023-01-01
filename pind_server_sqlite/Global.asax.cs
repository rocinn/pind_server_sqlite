using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;
using System.Web.SessionState;
using System.Web.Http;
using pind_server_sqlite.App_Start;
using System.Web.Optimization;
using log4net.Config;
using log4net;
using System.IO;

namespace pind_server_sqlite
{
    public class Global : HttpApplication
    {
        void Application_Start(object sender, EventArgs e)
        {
            // 在应用程序启动时运行的代码
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            RouteConfig.RegisterRoutes(RouteTable.Routes);

            BundleConfig.RegisterBundles(BundleTable.Bundles);

            GlobalFilters.Filters.Add(new MyAuthorizationFilter());

            using (FileStream fs = File.Open(AppDomain.CurrentDomain.BaseDirectory + "log4net.xml", FileMode.Open))
            {
                XmlConfigurator.Configure(fs);
                ILog m_log = LogManager.GetLogger("Application_Start");
                m_log.Debug("这是一个启动Debug日志");
            }
        }
    }
}