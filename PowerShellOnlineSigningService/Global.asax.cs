using System;
using System.Web;

namespace PowerShellOnlineSigningService
{
    public class Global : System.Web.HttpApplication
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger("Global.asax");

        protected void Application_PreSendRequestHeaders(object sender, EventArgs e)
        {

        }

        protected void Application_BeginRequest(Object sender, EventArgs e)
        {
            // Keep in place to show render time in response header via the ServerHeaderModule
            HttpContext.Current.Items["renderStartTime"] = DateTime.Now;
        }

        void Application_EndRequest(object sender, EventArgs e)
        {
            DateTime start = (DateTime)HttpContext.Current.Items["renderStartTime"];
            TimeSpan renderTime = DateTime.Now - start;

            string prettyTime = string.Concat((renderTime.Hours > 0 ? renderTime.Hours + "h " : string.Empty),
                                        (renderTime.Minutes > 0 ? renderTime.Minutes + "m " : string.Empty),
                                        (renderTime.Seconds > 0 ? renderTime.Seconds + "s " : string.Empty),
                                        (renderTime.Milliseconds > 0 ? renderTime.Milliseconds + "ms " : string.Empty));

            log.DebugFormat("[{0}] Render time: {1}", HttpContext.Current.Request.Url.AbsoluteUri, prettyTime);
        }

        void Application_Start(object sender, EventArgs e)
        {
            System.IO.FileInfo logfile = new System.IO.FileInfo(Server.MapPath("log4net.config"));
            log4net.Config.XmlConfigurator.ConfigureAndWatch(logfile);
        }

        void Application_End(object sender, EventArgs e)
        {
            log4net.LogManager.Shutdown();
        }

        void Application_Error(object sender, EventArgs e)
        {
            // An error has occured on a .Net page.
            var serverError = Server.GetLastError() as HttpException;

            if (null != serverError)
            {
                int errorCode = serverError.GetHttpCode();

                if (404 == errorCode)
                {
                    Server.ClearError();
                    Server.Transfer("/Lost.aspx");
                }
                else
                {
                    Server.ClearError();
                    Server.Transfer("/Oops.aspx");
                }            
            }
        
        }

        void Session_Start(object sender, EventArgs e)
        {
        }

        void Session_End(object sender, EventArgs e)
        {
        }
    }
}