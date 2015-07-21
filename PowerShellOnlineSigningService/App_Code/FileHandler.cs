using log4net;
using System;
using System.IO;
using System.Reflection;
using System.Web;

public class FileHandler : IHttpHandler
{
    private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
    private string scriptFolder = "~/files";

    public bool IsReusable
    {
        get { return false; }
    }
    
    public void ProcessRequest(HttpContext context)
    {
        if (!string.IsNullOrEmpty(context.Request.QueryString["file"]))
        {
            try
            {
                string folder = context.Server.MapPath(scriptFolder);
                FileInfo file = new FileInfo(folder + Path.DirectorySeparatorChar + context.Request.QueryString["file"]);

                if (file.Exists && file.Directory.FullName.StartsWith(folder, StringComparison.OrdinalIgnoreCase))
                {
                    context.Response.Clear();
                    context.Response.Cache.SetCacheability(HttpCacheability.NoCache);
                    context.Response.ContentType = "text/plain";
                    context.Response.AddHeader("Content-Length", file.Length.ToString());
                    context.Response.AppendHeader("Content-Disposition", "attachment; filename=\"" + file.Name + "\"");
                    context.Response.TransmitFile(file.FullName);

                    log.DebugFormat("File downloaded: {0}", file.Name);
                }
                else
                {
                    log.DebugFormat("An attempt was made to download an unknown file: {0}", file.Name);
                    context.Response.StatusCode = 404;
                    context.Response.StatusDescription = "Unable to locate the request file";
                }
            }
            catch (Exception ex)
            {
                log.Error(ex);
                context.Response.StatusCode = 500;
                context.Response.StatusDescription = "Internal Server Error";
            }
        }
    }
}