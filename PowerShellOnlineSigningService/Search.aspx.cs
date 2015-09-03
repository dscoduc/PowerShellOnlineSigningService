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
    public partial class Search : System.Web.UI.Page
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Page.IsPostBack)
                return;

            string searchString = SecurityElement.Escape(HttpContext.Current.Request.QueryString["s"]);

            try
            {
                loadResults(searchString);
            }
            catch (ThreadAbortException)
            {
                // do nothing - it's needed to handle Response.Redirect
            }

            displayBreadcrumb(searchString);
        }

        private void displayBreadcrumb(string searchString)
        {
            string urlTemplate = "<a href='{0}'>{1}</a>";
            string homeURL = string.Format(urlTemplate, "Default.aspx", "Home");

            string currentPage = string.Format(urlTemplate, "Search.aspx", "Search Users");

            string searchCriteria = string.Format(urlTemplate, "Search.aspx?s=" + searchString, searchString);

            string breadcrumb = string.Format("{0} / {1} / {2}", homeURL, currentPage, searchCriteria);
            HtmlGenericControl site_breadcrumb = (HtmlGenericControl)Master.FindControl("site_breadcrumb");
            site_breadcrumb.Visible = true;
            site_breadcrumb.InnerHtml = breadcrumb;
        }

        private void loadResults(string searchString)
        {

            if (string.IsNullOrEmpty(searchString))
                return;

            // array to hold results
            List<string> items = new List<string>();

            List<GitUserDetails> Users = GitHubClient.GetUsers(searchString);
            if (null == Users || Users.Count < 1)
            {
                items.Add(string.Format("<li>No matching items found for {0}...</li>", searchString));
            }
            else if (Users.Count == 1)
            {
                string url = string.Format("User.aspx?owner={0}", Users[0].login);
                Response.Redirect(url, false);
                Context.ApplicationInstance.CompleteRequest();
            }
            else
            {
                foreach (GitUserDetails user in Users)
                {
                    string login = SecurityElement.Escape(user.login);
                    string userUrl = string.Format("User.aspx?owner={0}", login);
                    string formattedUsername = (string.IsNullOrEmpty(user.name)) ? 
                        string.Empty : string.Format("({0})", SecurityElement.Escape(user.name));

                    items.Add("<li class='users'>" +
                                "<a href='" + userUrl + "'>" +
                                    "<img class='avatar' src='" + user.avatar_url + "&s=60'>" +  
                                    "<p>" + login + "</p>" +
                                    "<p>" + formattedUsername + "</p>" +
                                "</a>" + 
                              "</li>");
                }
            }

            // combine all of the entries
            string results = string.Join("", items.ToArray());

            //TODO: update results placeholder with results
            HtmlGenericControl phResults = (HtmlGenericControl)Master.FindControl("cphBody").FindControl("results");
            phResults.InnerHtml = "<ul class='userList'>" + results + "</ul>";
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