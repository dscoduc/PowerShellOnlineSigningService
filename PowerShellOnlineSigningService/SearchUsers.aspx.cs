using GitHubAPIClient;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
        private string searchString = HttpContext.Current.Request.QueryString["s"] ?? string.Empty;


        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                try
                {
                    gvUsers_LoadData();
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

        private void gvUsers_LoadData()
        {
            List<GitUserDetails> Users = null;

            if (string.IsNullOrEmpty(searchString))
                return;

            try { Users = GitHubClient.GetUsers(searchString); }
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
                string url = string.Format("~/UserContent.aspx?owner={0}", Users[0].login);
                Response.Redirect(url, false);
                Context.ApplicationInstance.CompleteRequest();
            }

            GridView gvUsers = (GridView)Master.FindControl("cphBody").FindControl("gvUsers");
            gvUsers.DataSource = Users;
            gvUsers.DataBind();
        }

        protected void gvUsers_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType != DataControlRowType.DataRow) { return; }

            GitUserDetails user = (GitUserDetails)e.Row.DataItem;
            if (null == user) { return; }

            string formattedUsername = (string.IsNullOrEmpty(user.name)) ? string.Empty : string.Format("<br />({0})", user.name);

            ((Image)e.Row.FindControl("avatarURL")).ImageUrl = string.Format("{0}&s=60", user.avatar_url);

            ((HyperLink)e.Row.FindControl("contentLink")).NavigateUrl = string.Format("~/UserContent.aspx?owner={0}", user.login);
            ((HyperLink)e.Row.FindControl("contentLink")).ToolTip = "Click to view GitHub Repositories";
            ((HyperLink)e.Row.FindControl("contentLink")).Text = string.Format("{0}{1}", user.login, formattedUsername);

            // hidden fields for later use maybe
            ((Label)e.Row.FindControl("Login")).Text = user.login;
            ((Label)e.Row.FindControl("userURL")).Text = user.url;

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