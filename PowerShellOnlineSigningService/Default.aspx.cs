using GitHubAPIClient;
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
        private static string defaultOwner = ConfigurationManager.AppSettings["default_owner"].ToString();
        private static string defaultRepository = ConfigurationManager.AppSettings["default_repository"].ToString();
        private string approvedExtensions = @"^.+\.((ps1)|(txt))$";
        private Requestor requestor = new Requestor();

        protected void Page_Load(object sender, EventArgs e)
        {
            // displays username and servername
            displaySessionInfo();

            // if page is postback then skip the rest of the steps below
            if (Page.IsPostBack)
                return;

            // attempt to populate owner values with either querystring or default value
            if (!string.IsNullOrEmpty(Request.QueryString["owner"]))
            {
                Session["GitOwner"] = tbRepoOwner.Text = Server.HtmlEncode(Request.QueryString["owner"]);

                // populate the repository if provided in querystring
                if (!string.IsNullOrEmpty(Request.QueryString["repository"]))
                    Session["GitRepository"] = Server.HtmlEncode(Request.QueryString["repository"]);
            }
            else if (!string.IsNullOrEmpty(defaultOwner))
            {
                Session["GitOwner"] = tbRepoOwner.Text = defaultOwner;

                // populate the repository if provided in default values
                if (!string.IsNullOrEmpty(defaultRepository))
                    Session["GitRepository"] = defaultRepository;            
            }

            // load repository dropdown list
            if(populateRepoList())
                displayFileList();
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

        private bool populateRepoList()
        {
            ddlRepositories.Items.Clear();

            string cachedOwner = (string)Session["GitOwner"];
            string cachedRepoName = (string)Session["GitRepository"];

            if(string.IsNullOrEmpty(cachedOwner))
            {
                log.Info("Skipping population of repository list due to unassigned owner value");
                return false;
            }

            List<GitRepository> Repositories = null;
            try
            {
                Repositories = GitHubClient.GetRepositories(cachedOwner);
            }
            catch (Exception) { } // Continue - Repositories will be null and will exit gracefully

            if (null == Repositories || Repositories.Count < 1)
            {
                log.InfoFormat("Notifying user no repositories were found for {0}", cachedOwner);
                resultsInfo.InnerHtml = string.Format("<span class='error'>No repositories found for the repository owner {0}</span>", cachedOwner);

                // clear out repository cache info
                Session.Remove("GitRepository");

                return false;
            }

            foreach (GitRepository repository in Repositories)
            {
                string cleanRepoName = Server.HtmlEncode(repository.name);

                log.DebugFormat("Adding {0} to repository list", cleanRepoName);

                ListItem item = new ListItem(cleanRepoName);
                
                // check if there is already a specified repository to select
                if (!string.IsNullOrEmpty(cachedRepoName) && string.Compare(cleanRepoName, cachedRepoName, true) == 0)
                    item.Selected = true;

                ddlRepositories.Items.Add(item);
            }

            Session["GitRepository"] = ddlRepositories.SelectedItem.Text;

            return true;
        }

        private void displayFileList()
        {
            // Clear existing file list
            tblFileList.Rows.Clear();

            // fetch cached information
            string cachedOwner = (string)Session["GitOwner"];
            string cachedRepoName = (string)Session["GitRepository"];

            // bail if the cached repository info is empty
            if (string.IsNullOrEmpty(cachedRepoName))
            {
                log.Info("Skipping population of file list due to unassigned repository value");
                return;
            }

            List<GitContent> contents = null;
            try
            {
                // retrieve list of contents in Repository
                contents = GitHubClient.GetContents(cachedOwner, cachedRepoName);
            }
            catch (Exception) { } // Continue - contents will be null and will exit gracefully

            // if null or empty the lookup failed
            if (null == contents || contents.Count < 1)
            {
                log.InfoFormat("Notifying user no objects were found in {0}", cachedRepoName);
                resultsInfo.InnerText = string.Format("Unable to find any objects in {0}", cachedRepoName);

                return;
            }

            // loop through each entry returned for repository
            foreach (GitContent entry in contents)
            {
                // check if we should be displaying the entry
                if (Regex.IsMatch(entry.name, approvedExtensions))
                {
                    log.DebugFormat("Adding qualified file {0}", entry.name);

                    #region AddDataRow
                    using (TableRow row = new TableRow())
                    {
                        TableCell cell;

                        using (cell = new TableCell())
                        {
                            cell.Text = string.Format("<a href='DownloadFile.ashx?owner={0}&repository={1}&file={2}' title='Click to download'>{2}</a>", cachedOwner, cachedRepoName, entry.name);
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

            // if no rows were added then we don't need the file list header
            if (tblFileList.Rows.Count == 0) 
            {
                log.InfoFormat("Notifying user no qualifying files were found for {0} repository", cachedRepoName);
                resultsInfo.InnerHtml = string.Format("<span class='error'>Unable to find any qualifying files for {0} repository</span>", cachedRepoName);    
                return; 
            }

            #region AddHeader
            // We found qualifying entries - let's show the header
            using (TableHeaderRow headerRow = new TableHeaderRow())
            {
                TableHeaderCell headerCell;

                // add first column
                using (headerCell = new TableHeaderCell())
                {
                    headerCell.Width = System.Web.UI.WebControls.Unit.Pixel(450);
                    headerCell.Text = "File Name";
                    headerCell.HorizontalAlign = HorizontalAlign.Left;
                    headerRow.Cells.Add(headerCell);
                }

                // add second column
                using (headerCell = new TableHeaderCell())
                {
                    headerCell.Text = "File Size";
                    headerCell.Width = System.Web.UI.WebControls.Unit.Pixel(100);
                    headerCell.HorizontalAlign = HorizontalAlign.Right;
                    headerCell.Style.Add("padding-right", "10px;");
                    headerRow.Cells.Add(headerCell);
                }

                // add header row to table
                tblFileList.Rows.AddAt(0, headerRow);
            }
            #endregion // AddHeader
        }

        protected void btnRefreshList_Click(object sender, EventArgs e)
        {
            // clear any previous result messages
            resultsInfo.InnerHtml = "";

            // clear everything if Owner value is empty
            if (string.IsNullOrEmpty(tbRepoOwner.Text))
            {
                log.Info("Repository owner information was emptied by user");

                ddlRepositories.Items.Clear();
                tblFileList.Rows.Clear();

                // if there are no default variables then clear out everything
                if (string.IsNullOrEmpty(defaultOwner))
                {
                    // empty cache info
                    Session.Remove("GitOwner");
                    Session.Remove("GitRepository");

                    return;
                }

                // set the owner cache with default cvalue
                Session["GitOwner"] = tbRepoOwner.Text = defaultOwner;

                // check if default repository is available
                if (string.IsNullOrEmpty(defaultRepository))
                    Session.Remove("GitRepository");
                else
                    Session["GitRepository"] = defaultRepository;

            }

            string inputRepoOwner = Server.HtmlEncode(tbRepoOwner.Text);
            string cachedRepoOwner = (string)Session["GitOwner"];

            // check if owner name has changed
            if (string.Compare(inputRepoOwner, cachedRepoOwner, true) != 0)
            {
                log.InfoFormat("Repository owner changed from {0} to {1}", cachedRepoOwner, inputRepoOwner);
                
                // update owner session with new owner input
                Session["GitOwner"] = Server.HtmlEncode(tbRepoOwner.Text);

                // clear out previous repository cache
                Session.Remove("GitRepository");
            }
            else
            {
                // update repository session with selected repository if available
                if (null != ddlRepositories.SelectedItem)
                    Session["GitRepository"] = Server.HtmlEncode(ddlRepositories.SelectedItem.Text);            
            }

            // repopulate Repository list
            if(populateRepoList())
                displayFileList();
        }
    }
}