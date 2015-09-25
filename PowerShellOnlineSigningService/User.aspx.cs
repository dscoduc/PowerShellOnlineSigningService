using GitHubAPIClient;
using log4net;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Reflection;
using System.Security;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

namespace PowerShellOnlineSigningService
{
    public partial class User : System.Web.UI.Page
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private string approvedExtensions = ConfigurationManager.AppSettings["approved_extensions"] ?? @"^.+\.((ps1)|(ps1))$";
        private static string defaultOwner = ConfigurationManager.AppSettings["default_owner"] ?? string.Empty;
        private static string defaultRepository = ConfigurationManager.AppSettings["default_repository"] ?? string.Empty;

        protected void Page_Load(object sender, EventArgs e)
        {
            string userName = (string)HttpContext.Current.Request.QueryString["o"] ?? null;
            string repoName = (string)HttpContext.Current.Request.QueryString["r"] ?? null;
            string requestPath = (string)HttpContext.Current.Request.QueryString["p"] ?? string.Empty;
            
            if (Page.IsPostBack)
                return;

            if (string.IsNullOrEmpty(userName) && !string.IsNullOrEmpty(defaultOwner))
            {
                if (string.IsNullOrEmpty(repoName) && !string.IsNullOrEmpty(defaultRepository))
                    Response.Redirect(string.Format("{0}?o={1}&r={2}", Request.Url.AbsolutePath, defaultOwner, defaultRepository), true);
                else
                    Response.Redirect(string.Format("{0}?o={1}", Request.Url.AbsolutePath, defaultOwner), true);
            }

            loadResults(userName, repoName, requestPath);

            if(!string.IsNullOrEmpty(userName))
                populateBreadCrumb(userName, repoName, requestPath);

        }

        private void loadResults(string userName, string repoName, string requestPath)
        {
            // array to hold results
            List<string> items = new List<string>();

            // string to hold message
            string outMessage = string.Empty;

            // only owner provided in request
            if (!string.IsNullOrEmpty(userName) && string.IsNullOrEmpty(repoName))
            {
                List<GitRepository> Repositories = GitHubClient.GetRepositories(userName);

                if (null == Repositories || Repositories.Count < 1)
                {
                    outMessage = string.Format("No repositories found for {0}...", userName);
                }
                else
                {
                    foreach (GitRepository repo in Repositories)
                    {
                        // extract the user item from the user field
				        string owner = SecurityElement.Escape(repo.name);
				        var description = SecurityElement.Escape(repo.description);
				        var urlPath = string.Format("?o={0}&r={1}", userName, repo.name);

				        // build html syntax for each user
                        items.Add("<li class='repo'>" +
								        "<a href='" + urlPath + "' tooltip='Click to see the contents'>" + 
									        "<h3 class='repoList_name'>" + owner + "</h3>" + 
								        "</a>" + 
								        "<p class='repoList_description'>" + description + "</p>" +
							        "</li>"
				        );
                    }
                }
            }
            // owner and repo provided
            else if (!string.IsNullOrEmpty(userName) && !string.IsNullOrEmpty(repoName))
            {
                List<GitContent> unfilteredItems = GitHubClient.GetContents(userName, repoName, requestPath);
                if (null == unfilteredItems || unfilteredItems.Count < 1)
                {
                    outMessage = "No approved file types found...";
                }
                else
                {
                    // temp storage for filtering out unapproved file types
                    List<GitContent> allowedItems = new List<GitContent>();
                    foreach (GitContent entry in unfilteredItems)
                    {
                        string name = SecurityElement.Escape(entry.name);
                        string size = GetFileSizeString(entry.size);

                        if(entry.type == "file")
                        {
                            if (Regex.IsMatch(entry.name, approvedExtensions, RegexOptions.IgnoreCase))
                            {
                                // include approved file types
                                allowedItems.Add(entry);
                            }
                            else
                            {
                                // skip unapproved file types
                                log.DebugFormat("Skipping {0} - not on approved extension list", entry.name);
                            }
                        }
                        else
                        {
                            // include directories
                            allowedItems.Add(entry);                            
                        }

                        log.DebugFormat("Total of {0} approved files and folders", allowedItems.Count);
                    }

                    if (allowedItems.Count < 1)
                    {
                        outMessage = "No approved file types found...";
                    }
                    else
                    {
                        // loop though approved entries
                        foreach (GitContent item in allowedItems)
                        {
                            string urlPath = "#";
                            string name = item.name;
                            string size = GetFileSizeString(item.size);
                            string type = item.type ?? string.Empty;

                            // create url for file path
                            if (item.type == "file")
                            {
                                urlPath = string.Format("DownloadFile.ashx?o={0}&r={1}&p={2}", userName, repoName, item.path);
                            }
                            // create url for folder path
                            else
                            {
                                urlPath = string.Format("?o={0}&r={1}&p={2}", userName, repoName, item.path);
                                size = string.Empty;
                            }


                            // build html syntax for each user
				            items.Add("<li class='" + item.type + "'>" +
								            "<a href='" + urlPath + "'>" + 
									            "<span class='contentList_name'>" + name + "</span>" +
								            "</a>" + 
								            "<span class='contentList_size'>" + size + "</span>" +
							            "</li>");
                        }
                    }
                }
            }
            // everything else
            else 
            {
                outMessage = "Nothing found...";
            }

            if (!string.IsNullOrEmpty(outMessage)) {

                Label lblMessage = new Label();
                lblMessage.CssClass = "contentMessage";
                lblMessage.Text = outMessage;

                phMessage.Controls.Add(lblMessage);

            }
            else {

                phResults.Controls.Add(new LiteralControl("<ul class='contentList'>"));

                foreach (var item in items)
                    phResults.Controls.Add(new LiteralControl(item));
                
                phResults.Controls.Add(new LiteralControl("</ul>"));

            }
        }

        private void populateBreadCrumb(string userName, string repoName, string requestPath)
        {
            PlaceHolder phBreadCrumbList = (PlaceHolder)Master.FindControl("crumbsPlaceHolder");
            phBreadCrumbList.Controls.Add(new LiteralControl("<ul class='crumbs'>"));

            List<string> items = new List<string>();

            items.Add("<li><a href='Default.aspx'>Home</a></li>");

            if (!string.IsNullOrEmpty(userName))
            {
                items.Add(string.Format("<li><a href='User.aspx?o={0}'>{0}</a></li>", userName));

                if (!string.IsNullOrEmpty(repoName))
                {
                    items.Add(string.Format("<li><a href='User.aspx?o={0}&r={1}'>{1}</a></li>", userName, repoName));
                    

                    // check if there is a request path 
                    if (!string.IsNullOrEmpty(requestPath))
                    {
                        // need a temporary list
                        List<string> t = new List<string>();
                        string urlTemplate = "<li><a href='?o=" + userName + "&r=" + repoName + "&p={0}'>{1}</a></li>";

                        // enumerate through sub-folders in reverse to get the breadcrumb order correct
                        if (requestPath.Contains("/"))
                        {
                            do
                            {
                                // extract foldername from end of path
                                string folderName = requestPath.Substring(requestPath.LastIndexOf("/") + 1);
                                // build the request path
                                string item = string.Format(urlTemplate, requestPath, folderName);

                                //add it to temp list
                                t.Add(item);

                                // continue to advance through the paths until no more sub folders
                                requestPath = requestPath.Substring(0, requestPath.LastIndexOf("/"));

                            } while (requestPath.Contains("/"));
                        }

                        // add final path to the temp list
                        t.Add(string.Format(urlTemplate, requestPath, requestPath));

                        // loop in reverse to put the breadcrumb order correct
                        for (int x = t.Count - 1; x >= 0; x--)
                            items.Add(t[x]);
                    }
                }
            }
            else
            {
                items.Add("<li>User</li>");
            }

            // add each entry in items to the breadcrumb
            foreach (var item in items)
                phBreadCrumbList.Controls.Add(new LiteralControl(item));

            phBreadCrumbList.Controls.Add(new LiteralControl("</ul>"));
        }

        /// <summary>
        /// <para>Returns the human-readable file size for an arbitrary, 64-bit file size</para>
        /// <para>The default format is "0.### XB", e.g. "4.2 KB" or "1.434 GB"</para>
        /// </summary>
        private string GetFileSizeString(long i)
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
    }
}