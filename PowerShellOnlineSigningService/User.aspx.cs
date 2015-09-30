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
        private string approvedExtensions = ConfigurationManager.AppSettings["approved_extensions"] ?? @"^.+\.((ps1)|(PS1))$";
        private static string defaultOwner = ConfigurationManager.AppSettings["default_owner"] ?? string.Empty;
        private static string defaultRepository = ConfigurationManager.AppSettings["default_repository"] ?? string.Empty;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Page.IsPostBack)
                return;

            string userName = (string)HttpContext.Current.Request.QueryString["o"] ?? null;
            string repoName = (string)HttpContext.Current.Request.QueryString["r"] ?? null;
            string requestPath = (string)HttpContext.Current.Request.QueryString["p"] ?? string.Empty;
            
            // if no owner provided then try and revert to default info in web.config
            if (string.IsNullOrEmpty(userName) && !string.IsNullOrEmpty(defaultOwner))
                Response.Redirect(string.Format("{0}?o={1}&r={2}", Request.Url.AbsolutePath, defaultOwner, defaultRepository), true);

            // if owner is provided without repo then just load repositories
            if (!string.IsNullOrEmpty(userName) && string.IsNullOrEmpty(repoName))
                loadRepos(userName);

            // if owner and repository is provided then load the contents of the repository
            if (!string.IsNullOrEmpty(userName) && !string.IsNullOrEmpty(repoName))
                loadRepoContents(userName, repoName, requestPath);

            // display the breadcrumb menu
            if(!string.IsNullOrEmpty(userName))
                populateBreadCrumb(userName, repoName, requestPath);
        }

        private void loadRepos(string userName)
        {
            // retrieve the list of repos
            List<GitRepository> repoList = GitHubClient.GetRepositories(userName);

            // make sure it's not empty
            if (null == repoList || repoList.Count < 1)
            {
                phMessage.Controls.Add(new Label() { CssClass = "contentMessage", Text = string.Format("No repositories found for {0}...", userName) });
                return;
            }

            // open output list
            phResults.Controls.Add(new LiteralControl("<ul class='contentList'>"));

            repoList.ForEach(delegate(GitRepository item)
            {
                string urlPath = string.Format("?o={0}&r={1}", userName, item.name);

                // create literal control string
                string li = "<li class='repo'>" +
                                "<a href='" + urlPath + "' tooltip='Click to see the contents'>" +
                                    "<h3 class='repoList_name'>" + SecurityElement.Escape(item.name) + "</h3>" +
                                "</a>" +
                                "<p class='repoList_description'>" + SecurityElement.Escape(item.description) + "</p>" +
                            "</li>";

                // add entry to list
                phResults.Controls.Add(new LiteralControl(li));

            });

            // close output list
            phResults.Controls.Add(new LiteralControl("</ul>"));
        }

        private void loadRepoContents(string userName, string repoName, string requestPath)
        {
            // retrieve the list of contents
            List<GitContent> contentList = GitHubClient.GetContents(userName, repoName, requestPath);

            // make sure it's not empty
            if (null == contentList || contentList.Count < 1)
            {
                phMessage.Controls.Add(new Label() { CssClass = "contentMessage", Text = "No entries found..." });
                return;
            }

            // get list of directories from the response
            List<GitContent> dirList = contentList.FindAll(delegate(GitContent item) { return item.type == "dir"; });

            // get list of approved file types from response
            List<GitContent> fileList = contentList.FindAll(delegate(GitContent item)
            {
                return (item.type == "file" && Regex.IsMatch(item.name, approvedExtensions, RegexOptions.IgnoreCase));
            });

            log.DebugFormat("Total of {0} directories and {1} approved files", dirList.Count, fileList.Count);

            // check if no approved files or folders were found
            if (dirList.Count + fileList.Count < 1)
            {
                phMessage.Controls.Add(new Label() { CssClass = "contentMessage", Text = "No approved entries found..." });
                return;
            }

            // open output list
            phResults.Controls.Add(new LiteralControl("<ul class='contentList'>"));

            // add entry for each directory item
            dirList.ForEach(delegate(GitContent item)
            {
                // build url path for folder link
                string urlPath = string.Format("?o={0}&r={1}&p={2}", userName, repoName, item.path);

                // create literal control
                string li = "<li class='" + item.type + "'>" +
                                    "<a href='" + urlPath + "'>" +
                                        "<span class='contentList_name'>" + SecurityElement.Escape(item.name) + "</span>" +
                                    "</a>" +
                            "</li>";

                // add entry to list
                phResults.Controls.Add(new LiteralControl(li));
            });

            // add entry for each directory item
            fileList.ForEach(delegate(GitContent item)
            {
                // build url path for file link
                string urlPath = string.Format("DownloadFile.ashx?o={0}&r={1}&p={2}", userName, repoName, item.path);

                // create literal control
                string li = "<li class='" + item.type + "'>" +
                                    "<a href='" + urlPath + "'>" +
                                        "<span class='contentList_name'>" + SecurityElement.Escape(item.name) + "</span>" +
                                    "</a>" +
                                    "<span class='contentList_size'>" + GetFileSizeString(item.size) + "</span>" +
                                "</li>";

                // add entry to list
                phResults.Controls.Add(new LiteralControl(li));
            });

            // close output list
            phResults.Controls.Add(new LiteralControl("</ul>"));
        }

        private void populateBreadCrumb(string userName, string repoName, string requestPath)
        {
            PlaceHolder phBreadCrumbList = (PlaceHolder)Master.FindControl("crumbsPlaceHolder");
            
            // open output ul
            phBreadCrumbList.Controls.Add(new LiteralControl("<ul class='crumbs'><li><a href='Default.aspx'>Home</a></li>"));

            if (!string.IsNullOrEmpty(userName))
            {
                // add entry to output ul
                phBreadCrumbList.Controls.Add(new LiteralControl(string.Format("<li><a href='User.aspx?o={0}'>{0}</a></li>", userName)));

                if (!string.IsNullOrEmpty(repoName))
                {
                    // add entry to output ul
                    phBreadCrumbList.Controls.Add(new LiteralControl(string.Format("<li><a href='User.aspx?o={0}&r={1}'>{1}</a></li>", userName, repoName)));
                    
                    // check if there is a request path 
                    if (!string.IsNullOrEmpty(requestPath))
                    {
                        // need a temporary list so we can sort the sub-folders correctly
                        List<string> folderList = new List<string>();
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
                                folderList.Add(item);

                                // continue to advance through the paths until no more sub folders
                                requestPath = requestPath.Substring(0, requestPath.LastIndexOf("/"));

                            } while (requestPath.Contains("/"));
                        }

                        // add final path to the temp list
                        folderList.Add(string.Format(urlTemplate, requestPath, requestPath));

                        // loop in reverse to put the breadcrumb order correct then add entry to output ul
                        for (int x = folderList.Count - 1; x >= 0; x--)
                            phBreadCrumbList.Controls.Add(new LiteralControl(folderList[x]));

                        // empty the tempList
                        folderList.Clear();
                    }
                }
            }
            else
            {
                // add entry to output ul
                phBreadCrumbList.Controls.Add(new LiteralControl("<li>User</li>"));
            }

            // close output ul
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