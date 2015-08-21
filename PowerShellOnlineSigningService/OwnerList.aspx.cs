using GitHubAPIClient;
using log4net;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

namespace PowerShellOnlineSigningService
{
    public partial class OwnerList : System.Web.UI.Page
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private Requestor requestor = new Requestor();
        private List<GitUser> gitUsers = new List<GitUser>();
        private string searchString = HttpContext.Current.Request.QueryString["s"] ?? string.Empty;


        protected void Page_Load(object sender, EventArgs e)
        {
            displaySessionInfo();

            try
            {
                gvUsers_LoadData();
            }
            catch (Exception ex)
            {
                log.Error(ex);
                Response.Clear();
                Response.StatusCode = 404;
                Response.StatusDescription = "Unable to locate the requested information";
                return;
            }


            if (Page.IsPostBack)
            {

            }
            else
            {

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

            gvUsers.DataSource = Users;
            gvUsers.DataBind();
        }

        protected void gvUsers_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType != DataControlRowType.DataRow) { return; }

            GitUserDetails user = (GitUserDetails)e.Row.DataItem;
            if (null == user) { return; }

            ((Image)e.Row.FindControl("avatarURL")).ImageUrl = "~/images/github.jpg";

            ((HyperLink)e.Row.FindControl("contentLink")).NavigateUrl = string.Format("~/Default.aspx?owner={0}", user.login);
            ((HyperLink)e.Row.FindControl("contentLink")).ToolTip = "Click to view owner repositories";
            ((HyperLink)e.Row.FindControl("contentLink")).Text = user.login;

            ((Label)e.Row.FindControl("Name")).Text = user.name;
            ((Label)e.Row.FindControl("Email")).Text = user.email;
            ((Label)e.Row.FindControl("userURL")).Text = user.url;

        }

        protected void gvUsers_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            if (e != null && e.NewPageIndex > -1)
            {
                gvUsers.PageIndex = e.NewPageIndex;

                gvUsers.DataSource = gitUsers;
                gvUsers.DataBind();
            }
        }

    }
}