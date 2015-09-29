using System;
using System.Web;

namespace PowerShellOnlineSigningService
{
    /// <summary>
    /// Running header removal in HTTPModule so it applies to
    /// all requests (for example: .js, .zip, etc.)
    /// </summary>
    public sealed class serverHeaderModule : IHttpModule
    {
        public void Dispose()
        { }

        public void Init(HttpApplication context)
        { context.PreSendRequestHeaders += OnPreSendRequestHeaders; }

        void OnPreSendRequestHeaders(object sender, EventArgs e)
        {
            if (HttpContext.Current.Items["renderStartTime"] != null)
            {
                DateTime start = (DateTime)HttpContext.Current.Items["renderStartTime"];
                TimeSpan renderTime = DateTime.Now - start;

                string prettyTime = string.Concat((renderTime.Hours > 0 ? renderTime.Hours + "h " : string.Empty),
                                    (renderTime.Minutes > 0 ? renderTime.Minutes + "m " : string.Empty),
                                    (renderTime.Seconds > 0 ? renderTime.Seconds + "s " : string.Empty),
                                    (renderTime.Milliseconds > 0 ? renderTime.Milliseconds + "ms " : string.Empty));

                HttpContext.Current.Response.Headers.Set("X-Render-Time", prettyTime);
            }
            
            HttpContext.Current.Response.Headers.Remove("Server");
            HttpContext.Current.Response.Headers.Remove("X-AspNet-Version");
        }
    }
}