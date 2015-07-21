using System;
using System.Web;

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
        DateTime start = (DateTime)HttpContext.Current.Items["renderStartTime"];
        TimeSpan renderTime = DateTime.Now - start;

        string prettyTime = string.Concat((renderTime.Hours > 0 ? renderTime.Hours + "h " : string.Empty),
                            (renderTime.Minutes > 0 ? renderTime.Minutes + "m " : string.Empty),
                            (renderTime.Seconds > 0 ? renderTime.Seconds + "s " : string.Empty),
                            (renderTime.Milliseconds > 0 ? renderTime.Milliseconds + "ms " : string.Empty));

        HttpContext.Current.Response.Headers.Set("Render-Time", prettyTime);
        HttpContext.Current.Response.Headers.Remove("Server");
        HttpContext.Current.Response.Headers.Remove("X-Powered-By");
        HttpContext.Current.Response.Headers.Remove("X-AspNet-Version");
    } 
}