using System;
using System.Collections.Generic;
using GitHubAPIClient;
using log4net;
using System.Web;
using System.Reflection;
using System.IO;
using System.Text;
using System.Management.Automation;
using System.Collections.ObjectModel;

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
            string workingFile = string.Empty;

            if (!string.IsNullOrEmpty(context.Request.QueryString["file"]))
            {
                try
                {
                    //string repository = context.Request.QueryString["repo"];
                    string fileName = context.Request.QueryString["file"];

                    //string file = Server.MapPath(scriptFolder) + Path.DirectorySeparatorChar + fileName;
                    //File.WriteAllText(tempfile, content);
                    //File.Copy(tempfile, file, true);

                    // create temp file
                    workingFile = createPresignedFile(fileName);

                    // sign the script file
                    //bool result = signFile(workingFile.FullName);

                    if (signFile(workingFile))
                    {
                        context.Response.Clear();
                        context.Response.Cache.SetCacheability(HttpCacheability.NoCache);
                        context.Response.ContentType = "application/octet-stream";
                        //context.Response.AddHeader("Content-Length", workingFile.Length.ToString());
                        context.Response.AppendHeader("Content-Disposition", "attachment; filename=\"" + workingFile + "\"");
                        context.Response.TransmitFile(fileName);

                        log.DebugFormat("File downloaded: {0}", workingFile);
                    }
                    else
                    {
                        log.Warn("Unable to complete the signing");
                        context.Response.StatusCode = 500;
                        context.Response.StatusDescription = "Internal Server Error";
                    }

                    context.Response.End();
                }
                catch (Exception ex)
                {
                    log.Error(ex);
                    throw;
                }
                finally
                {
                    if (!string.IsNullOrEmpty(workingFile))
                        WebUtils.DeleteFile(workingFile);
                }
            }
            else  {
                context.Response.Redirect("~/");
            }
        }

        private string createPresignedFile(string fileName)
        {
            // create temp file
            string workingFile = Path.GetTempFileName();
            log.DebugFormat("Created temp file {0}", workingFile);

            // load decoded content from requested file
            string rawContent = GitHubClient.GetFileContents(fileName);

            using (StreamWriter writer = new StreamWriter(workingFile))
            { 
                using (StreamReader reader = new StreamReader(new MemoryStream(Encoding.UTF8.GetBytes(rawContent ?? ""))))
                {
                    string firstLine = reader.ReadLine();
                    if (firstLine.StartsWith("<#--"))
                    {
                        log.DebugFormat("Replacing pre-existing author header [{0}]", firstLine);
                        writer.WriteLine(string.Format("<#-- Digital Signing requested by {0} --#>", requestor.IIS_Auth_Name));
                    }
                    else
                    {
                        log.InfoFormat("Adding new author header to {0}", fileName);
                        writer.WriteLine(string.Format("<#-- Digital Signing requested by {0} --#>", requestor.IIS_Auth_Name));
                        writer.WriteLine("");
                        writer.WriteLine(firstLine);
                    }

                    while (!reader.EndOfStream)
                    {
                        string line = reader.ReadLine();

                        if (line.StartsWith("# SIG # Begin signature block"))
                        {
                            log.Info("Stripping off previous signature block");
                            break;  // reached the end of the file, strip off previous signature if found
                        }
                        else
                        {
                            writer.WriteLine(line);
                        }
                    }
                }
            }
            log.InfoFormat("Completed saving {0} into temp file [{1}]", fileName, workingFile);
            return workingFile;
        }

        private Boolean signFile(string filePath)
        {
            string commandSyntax = string.Format("Set-AuthenticodeSignature -FilePath '{0}' @(Get-ChildItem cert:\\LocalMachine\\My -codesign)[0]", filePath);

            try
            {
                using (PowerShell PowerShellInstance = PowerShell.Create())
                {
                    PowerShellInstance.AddScript(commandSyntax);

                    log.DebugFormat("PowerShell syntax '{0}'", commandSyntax);
                    Collection<PSObject> PSOutput = PowerShellInstance.Invoke();

                    if (PowerShellInstance.Streams.Error.Count > 0)
                    {
                        log.Error(PowerShellInstance.Streams.Error[0].ToString());
                        return false;
                    }

                    if (PSOutput.Count > 0)
                    {
                        string statusMessage = ((System.Management.Automation.Signature)(PSOutput[0].BaseObject)).StatusMessage;
                        log.DebugFormat("PowerShell output: {0} - {1}", filePath, statusMessage);
                        return true;
                    }
                    else
                    {
                        log.Debug("PowerShell did not complete successfully");
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(ex);
                throw;
            }
        }

    }
}