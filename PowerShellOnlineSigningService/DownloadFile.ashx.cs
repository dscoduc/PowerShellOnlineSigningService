using GitHubAPIClient;
using log4net;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Management.Automation;
using System.Reflection;
using System.Web;

namespace PowerShellOnlineSigningService
{
    /// <summary>
    /// Summary description for DownloadFile
    /// </summary>
    public class DownloadFile : IHttpHandler
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private Requestor requestor = new Requestor();

        public bool IsReusable
        { get { return false; } }


        public void ProcessRequest(HttpContext context)
        {
            string filePath = string.Empty;
            string owner = context.Server.HtmlEncode(context.Request.QueryString["owner"]);
            string repository = context.Server.HtmlEncode(context.Request.QueryString["repository"]);
            string contentPath = context.Server.HtmlEncode(context.Request.QueryString["path"]);

            try
            {
                if (string.IsNullOrEmpty(contentPath))
                {
                    log.Debug("Request made without a proper File query string value");
                    context.Response.Clear();
                    context.Response.StatusCode = 404;
                    return;
                }

                log.InfoFormat("Download request for {0} by {1}", contentPath, requestor.samAccountName);

                // load decoded content from requested file
                string rawContent = GitHubClient.GetFileContents(owner, repository, contentPath);
                if (string.IsNullOrEmpty(rawContent))
                {
                    context.Response.Clear();
                    context.Response.StatusCode = 404;
                    context.Response.StatusDescription = "Unable to locate the requested file";
                    return;
                }

                // extract filename from contentPath
                string contentFileName = Path.GetFileName(contentPath);

                // save contents to file to be downloaded
                filePath = context.Server.MapPath("~/App_Data") + Path.DirectorySeparatorChar + contentFileName;
                File.WriteAllText(filePath, rawContent);

                // clean any previos signing
                cleanFileToBeDownloaded(filePath);

                if (!signFile(filePath))
                    throw new Exception("Attempt to sign digital file failed");
                
                context.Response.Clear();
                context.Response.Cache.SetCacheability(HttpCacheability.NoCache);
                context.Response.ContentType = "application/octet-stream";
                context.Response.AddHeader("Content-Length", new FileInfo(filePath).Length.ToString());
                context.Response.AppendHeader("Content-Disposition", "attachment; filename=\"" + contentFileName + "\"");
                context.Response.TransmitFile(filePath);
                context.Response.Flush();

                log.InfoFormat("Downloaded of {0} completed for {1}", contentFileName, requestor.samAccountName);
            }
            catch (Exception ex)
            {
                log.Error(ex);
                context.Response.Clear();
                context.Response.StatusCode = 500;
                context.Response.StatusDescription = "An unknown error occurred";
            }
            finally
            {
                // remove any remaining files
                Utils.DeleteFile(filePath);

                // close response on the way out...
                context.Response.End();
            }
        }

        private Boolean signFile(string filePath)
        {
            string commandSyntax = string.Format("Set-AuthenticodeSignature -FilePath '{0}' @(Get-ChildItem cert:\\LocalMachine\\My -codesign)[0]", filePath);

            using (PowerShell PowerShellInstance = PowerShell.Create())
            {
                PowerShellInstance.AddScript(commandSyntax);
                log.DebugFormat("Executing PowerShell syntax '{0}'", commandSyntax);

                // execute PowerShell script
                Collection<PSObject> PSOutput = PowerShellInstance.Invoke();

                // check if there were any PowerShell errors thrown
                if(PowerShellInstance.HadErrors)
                {
                    log.WarnFormat("PowerShell did not complete successfully \r\n{0}", PowerShellInstance.Streams.Error[0].ToString());
                    return false;
                }

                string statusMessage = ((System.Management.Automation.Signature)(PSOutput[0].BaseObject)).StatusMessage;
                log.DebugFormat("PowerShell status message: {0}", statusMessage);

                // return results of status message comparison
                return (statusMessage == "Signature verified.");
            }
        }

        private void cleanFileToBeDownloaded(string fileToBeDownloaded)
        {
            string tempfile = Path.GetTempFileName();
            log.DebugFormat("Created temp file {0}", tempfile);

            using (StreamWriter writer = new StreamWriter(tempfile))
            using (StreamReader reader = new StreamReader(fileToBeDownloaded))
            {
                string firstLine = reader.ReadLine();
                if (firstLine.StartsWith("<#--"))
                {
                    log.DebugFormat("Replacing pre-existing author header [{0}]", firstLine);
                    writer.WriteLine(string.Format("<#-- Digital Signing requested by {0} --#>", requestor.IIS_Auth_Name));
                }
                else
                {
                    log.DebugFormat("Adding new author header to {0}", fileToBeDownloaded);
                    writer.WriteLine(string.Format("<#-- Digital Signing requested by {0} --#>", requestor.IIS_Auth_Name));
                    writer.WriteLine(firstLine);
                }

                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();


                    if (line.StartsWith("# SIG # Begin signature block"))
                    {
                        log.Debug("Stripping off previous signature block");
                        break;  // reached the end of the file, strip off previous signature if found
                    }
                    else
                    {
                        writer.WriteLine(line);
                    }
                }
            }

            File.Copy(tempfile, fileToBeDownloaded, true);
            log.DebugFormat("Finished updating {0}", fileToBeDownloaded);

            if (!string.IsNullOrEmpty(tempfile))
                Utils.DeleteFile(tempfile);
        }
    }
}