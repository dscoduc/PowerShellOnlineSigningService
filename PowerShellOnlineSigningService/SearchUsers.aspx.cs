using GitHubAPIClient;
using log4net;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Security;
using System.Threading;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

namespace PowerShellOnlineSigningService
{
    public partial class SearchUsers : System.Web.UI.Page
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private List<GitUser> gitUsers = new List<GitUser>();

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                try
                {
                    loadData();
                }
                catch (ThreadAbortException)
                {
                    // do nothing - it's needed to handle Response.Redirect
                }
                catch (Exception ex)
                {
                    log.Warn(ex);
                    Response.Clear();
                    Response.StatusCode = 404;
                    Response.StatusDescription = "Unable to locate the requested information";
                    return;
                }
            }
            displayBreadcrumb();
        }

        private void displayBreadcrumb()
        {
            string urlTemplate = "<a href='{0}'>{1}</a>";
            string homeURL = string.Format(urlTemplate, "Default.aspx", "Home");

            string currentPage = string.Format(urlTemplate, "SearchUsers.aspx", "Search Users");

            string breadcrumb = string.Format("{0} / {1}", homeURL, currentPage);
            HtmlGenericControl site_breadcrumb = (HtmlGenericControl)Master.FindControl("site_breadcrumb");
            site_breadcrumb.Visible = true;
            site_breadcrumb.InnerHtml = breadcrumb;
        }

        private void loadData()
        {

            string searchString = HttpContext.Current.Request.QueryString["s"];

            if (string.IsNullOrEmpty(searchString))
                return;

            List<GitUserDetails> Users = null;
            try 
            { 
                Users = GitHubClient.GetUsers(searchString); 
            }
            catch (Exception) { } // Continue - Repositories will be null and will exit gracefully

            if (null == Users || Users.Count < 1)
            {
                log.DebugFormat("Notifying matching users were found for {0}", searchString);

                GitUserDetails u = new GitUserDetails();
                u.login = "No matching owners were found...";
                u.avatar_url = "";
                Users.Add(u);
            }

            if (Users.Count == 1)
            {
                string url = string.Format("~/User.aspx?owner={0}", Users[0].login);
                Response.Redirect(url, false);
                Context.ApplicationInstance.CompleteRequest();
            }

            DataList dtlist = (DataList)Master.FindControl("cphBody").FindControl("dtUsers");
            dtlist.DataSource = Users;
            dtlist.DataBind();

        }

        protected void dtUsers_DataBound(object sender, DataListItemEventArgs e)
        { 
            GitUserDetails user = (GitUserDetails)e.Item.DataItem;

            string formattedUsername = (string.IsNullOrEmpty(user.name)) ? string.Empty : string.Format("<br />({0})", SecurityElement.Escape(user.name));

            ((Image)e.Item.FindControl("avatar")).ImageUrl = string.Format("{0}&s=60", user.avatar_url);

            ((HyperLink)e.Item.FindControl("avatarLink")).Text = string.Format("{0}{1}", user.login, formattedUsername);
            ((HyperLink)e.Item.FindControl("avatarLink")).NavigateUrl = string.Format("~/User.aspx?owner={0}", user.login);
            ((HyperLink)e.Item.FindControl("avatarLink")).ToolTip = "Click to view repositories";

        }

        protected void btnSearch_Click(object sender, EventArgs e)
        {
            TextBox tbSearch = (TextBox)Master.FindControl("cphBody").FindControl("tbSearch");
            if (string.IsNullOrEmpty(tbSearch.Text))
                return;

            Response.Redirect(string.Format("{0}?s={1}", Request.Url.AbsolutePath, tbSearch.Text), true);

        }
    
    }
}