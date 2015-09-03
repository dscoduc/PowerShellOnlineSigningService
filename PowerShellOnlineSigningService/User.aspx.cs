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

namespace PowerShellOnlineSigningService
{
    public partial class User : System.Web.UI.Page
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private string approvedExtensions = ConfigurationManager.AppSettings["approved_extensions"];
        private static string defaultOwner = ConfigurationManager.AppSettings["default_owner"] ?? string.Empty;
        private static string defaultRepository = ConfigurationManager.AppSettings["default_repository"] ?? string.Empty;

        protected void Page_Load(object sender, EventArgs e)
        {
            string userName = (string)HttpContext.Current.Request.QueryString["owner"] ?? null;
            string repoName = (string)HttpContext.Current.Request.QueryString["repository"] ?? null;
            string requestPath = HttpContext.Current.Request.QueryString["path"] ?? string.Empty;
            
            if (Page.IsPostBack)
                return;

            if (string.IsNullOrEmpty(userName) && !string.IsNullOrEmpty(defaultOwner))
            {
                if (string.IsNullOrEmpty(repoName) && !string.IsNullOrEmpty(defaultRepository))
                    Response.Redirect(string.Format("{0}?owner={1}&repository={2}", Request.Url.AbsolutePath, defaultOwner, defaultRepository), true);
                else
                    Response.Redirect(string.Format("{0}?owner={1}", Request.Url.AbsolutePath, defaultOwner), true);
            }

            loadResults(userName, repoName, requestPath);

            if(!string.IsNullOrEmpty(userName))
                displayBreadcrumb(userName, repoName, requestPath);

        } // Page_Load

        private void loadResults(string userName, string repoName, string requestPath)
        {
            // array to hold results
            List<string> items = new List<string>();

            // only owner provided in request
            if (!string.IsNullOrEmpty(userName) && string.IsNullOrEmpty(repoName))
            {
                List<GitRepository> Repositories = GitHubClient.GetRepositories(userName);

                if (null == Repositories || Repositories.Count < 1)
                {
                    items.Add(string.Format("<li class='empty_results'>No matching repositories found for {0}...</li>", userName));
                }
                else
                {
                    foreach (GitRepository repo in Repositories)
                    {
                        // extract the user item from the user field
				        string owner = SecurityElement.Escape(repo.name);
				        var description = SecurityElement.Escape(repo.description);
				        var urlPath = string.Format("?owner={0}&repository={1}", userName, repo.name);

				        // build html syntax for each user
                        items.Add("<li class='repo'>" +
								        "<a href='" + urlPath + "' tooltip='Click to see the contents'>" + 
									        "<h3 class='repoList_name'>" + owner + "</h3>" + 
								        "</a>" + 
								        "<p class='repoList_description'>" + description + "</p>" +
							        "</li>"
				        );
                    } // for each
                } // if/else
            }
            // owner and repo provided
            else if (!string.IsNullOrEmpty(userName) && !string.IsNullOrEmpty(repoName))
            {
                List<GitContent> unfilteredItems = GitHubClient.GetContents(userName, repoName, requestPath);
                if (null == unfilteredItems || unfilteredItems.Count < 1)
                {
                    items.Add(string.Format("<li class='empty_results'>No approved file types found in {0}...</li>", repoName));
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
                            if (Regex.IsMatch(entry.name, approvedExtensions))
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
                    } // foreach

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
                            urlPath = string.Format("DownloadFile.ashx?owner={0}&repository={1}&path={2}", userName, repoName, item.path);
                        }
                        // create url for folder path
                        else
                        {
                            urlPath = string.Format("?owner={0}&repository={1}&path={2}", userName, repoName, item.path);
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
            // everything else
            else 
            {
                items.Add("<li class='empty_results'>Nothing found...</li>");
            }

            // combine all of the entries
            string results = string.Join("", items.ToArray());
            
            //TODO: update results placeholder with results
            HtmlGenericControl phResults = (HtmlGenericControl)Master.FindControl("cphBody").FindControl("results");
            phResults.InnerHtml = "<ul class='contentList'>" + results + "</ul>";
            
        } // loadResults

        private void displayBreadcrumb(string userName, string repoName, string requestPath)
        {
            if (!Request.QueryString.HasKeys())
                return;

            string urlTemplate = "<a href='{0}'>{1}</a>";
            string homeURL = "<a href='Default.aspx'>Home</a>";

            string breadcrumb = string.Empty;

            if (!string.IsNullOrEmpty(userName))
            {
                string ownerURL = string.Format(urlTemplate, "?owner=" + userName, userName);

                breadcrumb = string.Format("{0} / {1}", homeURL, ownerURL);

                if (!string.IsNullOrEmpty(repoName))
                {
                    string repositoryURL = string.Format(urlTemplate, "?owner=" + userName + "&repository=" + repoName, repoName);
                    breadcrumb = string.Format("{0} / {1}", breadcrumb, repositoryURL);

                    if (!string.IsNullOrEmpty(requestPath))
                    {
                        breadcrumb = string.Format("{0} / {1} ", breadcrumb, buildPathBreadcrumb(userName, repoName, requestPath));
                    }
                }
            }

            HtmlGenericControl site_breadcrumb = (HtmlGenericControl)Master.FindControl("site_breadcrumb");
            site_breadcrumb.Visible = true;
            site_breadcrumb.InnerHtml = breadcrumb;
        }

        private string buildPathBreadcrumb(string userName, string repoName, string requestPath)
        {
            if (!Request.QueryString.HasKeys())
                return string.Empty;

            string urlTemplate = "<a href='?owner=" + userName + "&repository=" + repoName + "&path={0}'>{1}</a>";
            string breadcrumb = string.Empty;
            ArrayList al = new ArrayList();

            if (requestPath.Contains("/"))
            {
                do
                {
                    string folderName = requestPath.Substring(requestPath.LastIndexOf("/") + 1);
                    string link = string.Format(urlTemplate, requestPath, folderName);

                    al.Add(link);

                    // advance through the paths
                    requestPath = requestPath.Substring(0, requestPath.LastIndexOf("/"));

                } while (requestPath.Contains("/"));
            }

            al.Add(string.Format(urlTemplate, requestPath, requestPath));

            for (int x = al.Count - 1; x >= 0; x--)
            {
                breadcrumb = string.Format("{0} / {1}", breadcrumb, al[x]);
            }

            return breadcrumb.Substring(3);
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