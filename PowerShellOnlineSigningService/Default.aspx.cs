using log4net;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Management.Automation;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using GitHubAPIClient;

public partial class _Default : System.Web.UI.Page 
{
    private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
    private string approvedExtensions = @"^.+\.((ps1)|(ps1))$";
    private string scriptFolder = "~/files";
    private Requestor requestor = new Requestor();

    protected void Page_Load(object sender, EventArgs e)
    {
        // displays username and servername
        displaySessionInfo();

        if (!Page.IsPostBack)
        {
            displayFileList();
        }
    }

    private void displaySessionInfo()
    {
        HtmlGenericControl userInfo = (HtmlGenericControl)Page.FindControl("userInfo");
        if (userInfo != null)
            userInfo.InnerText = string.Format("Welcome {0}", requestor.samAccountName);

        HtmlGenericControl serverInfo = (HtmlGenericControl)Page.FindControl("serverInfo");
        if (serverInfo != null)
            serverInfo.InnerText = System.Environment.MachineName.ToLower();    
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
        catch (Exception)
        {
            log.WarnFormat("Unable to sign file: {0}", filePath);

            deleteFile(filePath);
            throw;
        }
    }

    private void WriteToFile(string file)
    {
        try
        {
            string tempfile = Path.GetTempFileName();
            log.DebugFormat("Created temp file {0}", tempfile);

            using (StreamWriter writer = new StreamWriter(tempfile))
            using (StreamReader reader = new StreamReader(file))
            {
                string firstLine = reader.ReadLine();
                if (firstLine.StartsWith("<#--"))
                {
                    log.DebugFormat("Replacing pre-existing author header [{0}]", firstLine);
                    writer.WriteLine(string.Format("<#-- Digital Signing requested by {0} --#>", requestor.IIS_Auth_Name));
                }
                else
                {
                    log.DebugFormat("Adding new author header to {0}", file);
                    writer.WriteLine(string.Format("<#-- Digital Signing requested by {0} --#>", requestor.IIS_Auth_Name));
                    writer.WriteLine("");
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

            File.Copy(tempfile, file, true);
            log.DebugFormat("Finished updating {0}", file);

            if(!string.IsNullOrEmpty(tempfile))
                deleteFile(tempfile);
        }
        catch (Exception ex)
        {
            log.Error(ex);
            throw;
        }
    }

    private void deleteFile(string filePath)
    {
        try
        {
            FileInfo file = new FileInfo(filePath);
            if (file.Exists)
            {
                file.Delete();
                log.DebugFormat("Deleted {0}", filePath);
            }
            else
            {
                log.WarnFormat("Unable to delete {0} - file not found", filePath);
            }
        }
        catch (Exception ex)
        {
            log.Error(ex);
        }
    }

    /// <summary>
    /// <para>Returns the human-readable file size for an arbitrary, 64-bit file size</para>
    /// <para>The default format is "0.### XB", e.g. "4.2 KB" or "1.434 GB"</para>
    /// </summary>
    public static string GetFileSizeString(long i)
    {
        long absolute_i = (i < 0 ? -i : i);
        string suffix;
        double readable;

        // GB is enough for a VCS I think
        if (absolute_i >= 0x40000000)
        {
            suffix = "GB";
            readable = (i >> 20);
        }
        else if (absolute_i >= 0x100000)
        {
            suffix = "MB";
            readable = (i >> 10);
        }
        else if (absolute_i >= 0x400)
        {
            suffix = "kB";
            readable = i;
        }
        else
        {
            return i.ToString("0 B");
        }
        // Divide by 1024 to get fractional value
        readable = readable / 1024;
        return readable.ToString("0.### ") + suffix;
    }

    private void handleRequest()
    {
        Dictionary<string, bool> dicResults = new Dictionary<string, bool>();
        HttpFileCollection postedFiles;

        try
        {
            // Get collection of files
            postedFiles = Request.Files;

            // enumerate through list of files
            for (int i = 0; i < postedFiles.Count; i++)
            {
                HttpPostedFile file = postedFiles[i];
                string fileName = Path.GetFileName(file.FileName);

                // only process allowed script file types
                if (Regex.IsMatch(file.FileName, approvedExtensions, RegexOptions.IgnoreCase))
                {
                    // get file path to save script
                    string filePath = Server.MapPath(scriptFolder) + Path.DirectorySeparatorChar + fileName;

                    // save unsigned script
                    log.DebugFormat("Attempting to upload {0}", filePath);
                    file.SaveAs(filePath);
                    log.DebugFormat("Uploaded {0}", filePath);

                    // insert author information and strip out any previous signature
                    WriteToFile(filePath);

                    // sign the script file
                    bool result = signFile(filePath);

                    if (dicResults.ContainsKey(fileName))
                    {
                        log.DebugFormat("Ignoring duplicate file name [{0}]", fileName);
                    }
                    else
                    {
                        dicResults.Add(fileName, result);
                    }
                }
                else
                {
                    log.DebugFormat("Ignoring non-ps1 file [{0}]", file.FileName);
                }
            }

            displayResults(dicResults);
        }
        catch (Exception ex)
        {
            log.Error(ex);
            displayResults(dicResults);
        }
        finally
        {
            dicResults = null;
            postedFiles = null;
        }
    }

    private void displayResults(Dictionary<string, bool> dicResults)
    {
        try
        {
            if (dicResults.Count > 0)
            {
                tblResults.Rows.Clear();

                #region AddHeader
                using (TableHeaderRow headerRow = new TableHeaderRow())
                {
                    TableHeaderCell headerCell;

                    using (headerCell = new TableHeaderCell())
                    {
                        headerCell.Width = System.Web.UI.WebControls.Unit.Percentage(70);
                        headerCell.Text = "Uploaded Scripts";
                        headerCell.HorizontalAlign = HorizontalAlign.Left;
                        headerRow.Cells.Add(headerCell);
                    }

                    using (headerCell = new TableHeaderCell())
                    {
                        headerCell.Text = "Results";
                        headerCell.HorizontalAlign = HorizontalAlign.Center;
                        headerRow.Cells.Add(headerCell);
                    }

                    tblResults.Rows.Add(headerRow);
                }
                #endregion // AddHeader

                foreach (KeyValuePair<string, bool> result in dicResults)
                {
                    #region AddDataRow
                    using (TableRow row = new TableRow())
                    {
                        TableCell cell;

                        using (cell = new TableCell())
                        {
                            cell.Text = string.Format("<a href='file.axd?file={0}' title='Click to download'>{0}</a>", result.Key);
                            row.Cells.Add(cell);
                        }

                        using (cell = new TableCell())
                        {
                            cell.Text = (result.Value) ? "Signature verified" : "Error Occurred";
                            cell.CssClass = (result.Value) ? "success" : "error";
                            cell.HorizontalAlign = HorizontalAlign.Center;
                            row.Cells.Add(cell);
                        }

                        tblResults.Rows.Add(row);
                    }
                    #endregion // AddDataRow
                }
            }
            else
            {
                resultsInfo.InnerHtml = "<span class='error'>Unable to sign any scripts.</span>";
            }

            displayFileList();
        }
        catch (Exception ex)
        {
            log.Error(ex);
            resultsInfo.InnerHtml = "<span class='error'>An unexpected error has occurred.</span>";
        }
    }

    private void displayFileList()
    {
        try
        {
            // Clear existing file list
            tblFileList.Rows.Clear();

            #region AddHeader
            using (TableHeaderRow headerRow = new TableHeaderRow())
            {
                TableHeaderCell headerCell;

                using (headerCell = new TableHeaderCell())
                {
                    headerCell.Width = System.Web.UI.WebControls.Unit.Pixel(450);
                    headerCell.Text = "File Name";
                    headerCell.HorizontalAlign = HorizontalAlign.Left;
                    headerRow.Cells.Add(headerCell);
                }

                using (headerCell = new TableHeaderCell())
                {
                    headerCell.Text = "Last Modified";
                    headerCell.Width = System.Web.UI.WebControls.Unit.Pixel(185);
                    headerCell.HorizontalAlign = HorizontalAlign.Left;
                    headerRow.Cells.Add(headerCell);
                }

                using (headerCell = new TableHeaderCell())
                {
                    headerCell.Text = "File Size";
                    headerCell.Width = System.Web.UI.WebControls.Unit.Pixel(100);
                    headerCell.HorizontalAlign = HorizontalAlign.Right;
                    headerCell.Style.Add("padding-right", "10px;");
                    headerRow.Cells.Add(headerCell);
                }

                tblFileList.Rows.Add(headerRow);
            }
            #endregion // AddHeader

            // load all of the files in the folder
            string[] files = Directory.GetFiles(Server.MapPath(scriptFolder));
            log.DebugFormat("File folder lookup [{0}] found {1} total .ps1 file(s)", Server.MapPath(scriptFolder), files.Length);

            foreach (string file in files)
            {
                FileInfo iFile = new FileInfo(file);

                if (Regex.IsMatch(iFile.Name, approvedExtensions))
                {
                    #region AddDataRow
                    using (TableRow row = new TableRow())
                    {
                        TableCell cell;

                        using (cell = new TableCell())
                        {
                            cell.Text = string.Format("<a href='file.axd?file={0}' title='Click to download'>{0}</a>", iFile.Name);
                            row.Cells.Add(cell);
                        }

                        using (cell = new TableCell())
                        {
                            cell.Text = string.Format("{0} {1}", iFile.LastWriteTime.ToShortDateString(), iFile.LastWriteTime.ToShortTimeString());
                            row.Cells.Add(cell);
                        }

                        using (cell = new TableCell())
                        {
                            cell.Text = string.Format("<span class='size' title='{1} bytes'>{0}</span>", GetFileSizeString(iFile.Length), iFile.Length);
                            row.Cells.Add(cell);
                        }

                        tblFileList.Rows.Add(row);
                    }
                    #endregion // AddDataRow
                }
                else
                {
                    log.DebugFormat("Ignoring {0} - not on approved extension list", iFile.Name);
                }
            }
        }
        catch (Exception ex)
        {
            log.Error(ex);
        }
    }

    protected void btnSignFile_Click(object sender, EventArgs e)
    {
        handleRequest();
    }

}