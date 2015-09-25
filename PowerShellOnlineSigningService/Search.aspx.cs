﻿using GitHubAPIClient;
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
            phBreadCrumbList.Controls.Add(new LiteralControl("<ul class='breadcrumbList'>"));
            List<string> items = new List<string>();

            items.Add("<li><a href='Default.aspx'>Home</a></li>");
            items.Add("<li><a href='Search.aspx'>Search</a></li>");

            if (!string.IsNullOrEmpty(searchString) && searchString != "a")
                items.Add(string.Format("<li><a href='Search.aspx?s={0}'>{1}</a></li>", searchString, searchString));

            foreach (var item in items)
                phBreadCrumbList.Controls.Add(new LiteralControl(item));

            phBreadCrumbList.Controls.Add(new LiteralControl("</ul>"));
        }

        private void loadResults(string searchString)
        {

            if (string.IsNullOrEmpty(searchString))
                return;

            // array to hold results
            List<string> items = new List<string>();

            // string to hold message
            string outMessage = string.Empty;

            List<GitUser> Users = GitHubClient.GetUsers(searchString);
            if (null == Users || Users.Count < 1)
            {
                outMessage = string.Format("No matching items found for {0}...", searchString);
            }
            else if (Users.Count == 1)
            {
                string url = string.Format("User.aspx?o={0}", Users[0].login);
                Response.Redirect(url, false);
                Context.ApplicationInstance.CompleteRequest();
            }
            else
            {
                foreach (GitUser user in Users)
                {
                    string userUrl = string.Format("User.aspx?o={0}", user.login);
                    string formattedUsername = SecurityElement.Escape(user.login);

                    items.Add("<li class='users'>" +
                                "<a href='" + userUrl + "'>" +
                                    "<img src='images/repository.jpg'>" +
                                    "<p class='name'>" + formattedUsername + "</p>" +
                                "</a>" + 
                              "</li>");
                }
            }

            if (!string.IsNullOrEmpty(outMessage))
            {
                Label lblMessage = new Label();
                lblMessage.CssClass = "contentMessage";
                lblMessage.Text = outMessage;

                phMessage.Controls.Add(lblMessage);
            }
            else
            {
                phResults.Controls.Add(new LiteralControl("<ul class='userList'>"));

                foreach (var item in items)
                    phResults.Controls.Add(new LiteralControl(item));

                phResults.Controls.Add(new LiteralControl("</ul>"));
            }
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