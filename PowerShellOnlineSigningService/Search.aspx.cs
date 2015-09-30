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

            // use provided search string or default value
            string searchString = SecurityElement.Escape(HttpContext.Current.Request.QueryString["s"]) ?? "a";

            try
            {
                loadResults(searchString);
            }
            catch (ThreadAbortException)
            {
                // do nothing - it's needed to handle Response.Redirect
            }

            populateBreadCrumb(searchString);
        }

        private void populateBreadCrumb(string searchString)
        {
            PlaceHolder phBreadCrumbList = (PlaceHolder)Master.FindControl("crumbsPlaceHolder");
            string literal = "<ul class='breadcrumbList'>" +
                                 "<li><a href='Default.aspx'>Home</a></li>" + 
                                 "<li><a href='Search.aspx'>Search</a></li>" + 
                                 string.Format("<li><a href='Search.aspx?s={0}'>{0}</a></li>", searchString) + 
                             "</ul>";

            phBreadCrumbList.Controls.Add(new LiteralControl(literal));
        }

        private void loadResults(string searchString)
        {
            if (string.IsNullOrEmpty(searchString))
                return;

            // retrieve the list of users
            List<GitUser> userList = GitHubClient.GetUsers(searchString);

            // make sure it's not empty
            if (null == userList || userList.Count < 1)
            {
                phMessage.Controls.Add(new Label() { 
                    CssClass = "contentMessage", Text = string.Format("No matching items found for {0}...", searchString) 
                });

                return;
            }
            
            // if only one matching owner then auto-redirect to User.aspx
            if (userList.Count == 1)
            {
                string url = string.Format("User.aspx?o={0}", userList[0].login);
                Response.Redirect(url, false);
                Context.ApplicationInstance.CompleteRequest();
                return;
            }

            // open output list
            phResults.Controls.Add(new LiteralControl("<ul class='userList'>"));

            userList.ForEach(delegate(GitUser item)
            {
                // sanatize login ID
                string loginID = SecurityElement.Escape(item.login);

                // create literal control string
                // "<li class='users' style='background-image: ../images/repository.jpg'>" +
                string li = "<li class='users'>" +
                                "<a href='" + string.Format("User.aspx?o={0}", loginID) + "'>" +
                                    "<p class='name'>" + loginID + "</p>" +
                                "</a>" + 
                            "</li>";

                // add entry to list
                phResults.Controls.Add(new LiteralControl(li));
            });

            // close output list
            phResults.Controls.Add(new LiteralControl("</ul>"));
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