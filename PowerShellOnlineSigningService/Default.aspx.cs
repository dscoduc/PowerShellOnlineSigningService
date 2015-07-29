﻿using GitHubAPIClient;
using log4net;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

namespace PowerShellOnlineSigningService
{
    public partial class Default : System.Web.UI.Page
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        //private string repository_owner = ConfigurationManager.AppSettings["repository_owner"].ToString();
        //private string repository_name = ConfigurationManager.AppSettings["repository_name"].ToString();
        private string approvedExtensions = @"^.+\.((ps1)|(ps1))$";
        private Requestor requestor = new Requestor();

        protected void Page_Load(object sender, EventArgs e)
        {
            // displays username and servername
            displaySessionInfo();

            if (!Page.IsPostBack)
            {
                tbRepoOwner.Text = ConfigurationManager.AppSettings["repository_owner"].ToString();
                tbRepository.Text = ConfigurationManager.AppSettings["repository_name"].ToString();

                displayFileList(tbRepoOwner.Text, tbRepository.Text);
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

        private void displayFileList(string owner, string repository)
        {
            try
            {
                // Clear existing file list
                tblFileList.Rows.Clear();

                // retrieve list of contents in Repository
                List<GitContent> contents = GitHubClient.GetContents(owner, repository);

                if (contents.Count < 1)
                {
                    HtmlGenericControl gcResultsInfo = (HtmlGenericControl)Page.FindControl("resultsInfo");
                    if (gcResultsInfo != null)
                        gcResultsInfo.InnerText = string.Format("Unable to find any matching files", string.Empty);

                    return;
                }

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
                        headerCell.Text = "File Size";
                        headerCell.Width = System.Web.UI.WebControls.Unit.Pixel(100);
                        headerCell.HorizontalAlign = HorizontalAlign.Right;
                        headerCell.Style.Add("padding-right", "10px;");
                        headerRow.Cells.Add(headerCell);
                    }

                    tblFileList.Rows.Add(headerRow);
                }
                #endregion // AddHeader




                foreach (GitContent entry in contents)
                {
                    // Console.WriteLine("{0} [{1}] [{2}]", entry.name, entry.FileSize, entry.download_url);
                    if (Regex.IsMatch(entry.name, approvedExtensions))
                    {
                        #region AddDataRow
                        using (TableRow row = new TableRow())
                        {
                            TableCell cell;

                            using (cell = new TableCell())
                            {
                                cell.Text = string.Format("<a href='DownloadFile.ashx?owner={0}&repository={1}&file={2}' title='Click to download'>{2}</a>", owner, repository, entry.name);
                                row.Cells.Add(cell);
                            }

                            using (cell = new TableCell())
                            {
                                cell.Text = string.Format("<span class='size' title='{1} bytes'>{0}</span>", WebUtils.GetFileSizeString(entry.size), entry.size);
                                row.Cells.Add(cell);
                            }

                            tblFileList.Rows.Add(row);
                        }
                        #endregion // AddDataRow
                    }
                    else
                    {
                        log.DebugFormat("Ignoring {0} - not on approved extension list", entry.name);
                    }
                }


            }
            catch (Exception ex)
            {
                log.Error(ex);
                throw;
            }

        }

        protected void btnRefreshList_Click(object sender, EventArgs e)
        {
            displayFileList(tbRepoOwner.Text, tbRepository.Text);
        }

    }
}