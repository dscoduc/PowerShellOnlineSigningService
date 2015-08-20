using GitHubAPIClient;
using log4net;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

namespace PowerShellOnlineSigningService
{
    public partial class Default : System.Web.UI.Page
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private Requestor requestor = new Requestor();
        private string approvedExtensions = @"^.+\.((ps1)|(txt))$";
        private List<GitObject> gitObjects = new List<GitObject>();
        private static string defaultOwner = ConfigurationManager.AppSettings["default_owner"] ?? string.Empty;
        private static string defaultRepository = ConfigurationManager.AppSettings["default_repository"] ?? string.Empty;
        private string requestOwner = HttpContext.Current.Request.QueryString["owner"] ?? string.Empty;
        private string requestRepository = HttpContext.Current.Request.QueryString["repository"] ?? string.Empty;
        private string requestPath = HttpContext.Current.Request.QueryString["path"] ?? string.Empty;

        protected void Page_Load(object sender, EventArgs e)
        {
            string redirectURL = "Default.aspx";

            if (Page.IsPostBack)
            {                
                if(string.IsNullOrEmpty(tbOwner.Text))
                {
                    Response.Redirect(redirectURL, true);
                }
                else
                {
                    Response.Redirect(string.Format("{0}?owner={1}", redirectURL, tbOwner.Text), true);
                }
            }
            else
            {
                if (string.IsNullOrEmpty(requestOwner) && !string.IsNullOrEmpty(defaultOwner))
                {
                    if (string.IsNullOrEmpty(requestRepository) && !string.IsNullOrEmpty(defaultRepository))
                        Response.Redirect(string.Format("{0}?owner={1}&repository={2}", redirectURL, defaultOwner, defaultRepository), true);
                    else
                        Response.Redirect(string.Format("{0}?owner={1}", redirectURL, defaultOwner), true);
                }

                displaySessionInfo();

                try
                {
                    gvFiles_LoadData();
                }
                catch (Exception)
                {
                    Response.Clear();
                    Response.StatusCode = 404;
                    Response.StatusDescription = "Unable to locate the requested information";
                    return;
                }

                displayBreadcrumb();
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

        private void displayBreadcrumb()
        {
            if (!Request.QueryString.HasKeys()) { return; }

            string urlTemplate = "<a href='{0}'>{1}</a>";
            
            string pathURL = string.Format(urlTemplate, "?owner=" + requestOwner + "&repository=" + requestRepository + "&path=" + requestPath, requestPath);

            string breadcrumb = string.Empty;

            if (!string.IsNullOrEmpty(requestOwner))
            {
                string ownerURL = string.Format(urlTemplate, "?owner=" + requestOwner, requestOwner);
                breadcrumb = ownerURL;

                if (!string.IsNullOrEmpty(requestRepository))
                {
                    string repositoryURL = string.Format(urlTemplate, "?owner=" + requestOwner + "&repository=" + requestRepository, requestRepository);
                    breadcrumb = string.Format("{0} / {1}", ownerURL, repositoryURL);

                    if (!string.IsNullOrEmpty(requestPath))
                    {
                        breadcrumb = string.Format("{0} / {1} / {2}", ownerURL, repositoryURL, buildPathBreadcrumb());
                    }
                }
            }
            currentPath.InnerHtml = breadcrumb;
        }

        private string buildPathBreadcrumb()
        {
            if (!Request.QueryString.HasKeys()) { return string.Empty; }

            string urlTemplate = "<a href='?owner=" + requestOwner + "&repository=" + requestRepository + "&path={0}'>{1}</a>";
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

        private void gvFiles_LoadData()
        {
            if (string.IsNullOrEmpty(requestOwner)) 
            {
                GitObject m = new GitObject();
                m.name = "Specify a GitHub account to view the available repositories...";
                m.type = "github";
                gitObjects.Add(m);
            }
            else if (string.IsNullOrEmpty(requestRepository))
            {
                List<GitRepository> Repositories = null;
                
                try { Repositories = GitHubClient.GetRepositories(requestOwner); }
                catch (Exception) { } // Continue - Repositories will be null and will exit gracefully

                if (null == Repositories || Repositories.Count < 1)
                {
                    log.InfoFormat("Notifying user no repositories were found for {0}", requestOwner);
                    GitObject m = new GitObject();
                    m.name = "No repositories found...";
                    m.type = "github";
                    gitObjects.Add(m);
                }
                else
                {
                    foreach (GitRepository entry in Repositories)
                        gitObjects.Add(new GitObject(entry));
                }
            }
            else
            {
                List<GitContent> unfilteredContents = GitHubClient.GetContents(requestOwner, requestRepository, requestPath);

                foreach (GitContent entry in unfilteredContents)
                {
                    GitObject m = new GitObject(entry);
                    if (entry.type == "dir")
                    {
                        m.name = entry.name;
                        m.path = entry.path;
                        m.type = entry.type;

                        gitObjects.Add(m);
                    }
                    else
                    {
                        if (Regex.IsMatch(entry.name, approvedExtensions))
                        {
                            //gitContents.Add(entry); 
                            m.name = entry.name;
                            m.path = entry.path;
                            m.size = entry.size;
                            m.type = entry.type;

                            gitObjects.Add(m);
                        }
                    }
                }
            }

            gvFiles.DataSource = gitObjects;
            gvFiles.DataBind();        
        }

        protected void gvFiles_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType != DataControlRowType.DataRow) { return; }

            GitObject entry = (GitObject)e.Row.DataItem;
            if (null == entry) { return; }
                
            if (entry.type == "dir")
            {
                ((Image)e.Row.FindControl("typeImage")).ImageUrl = "~/images/folder.jpg";
                        
                ((HyperLink)e.Row.FindControl("contentLink")).NavigateUrl = string.Format("?owner={0}&repository={1}&path={2}", requestOwner, requestRepository, entry.path);
                ((HyperLink)e.Row.FindControl("contentLink")).ToolTip = "Click to list folder contents";

                ((Label)e.Row.FindControl("Size")).Text = string.Empty;

                ((Label)e.Row.FindControl("Path")).Text = entry.path;
            }
            else if (entry.type == "file")
            {
                ((Image)e.Row.FindControl("typeImage")).ImageUrl = "~/images/file.jpg";

                ((HyperLink)e.Row.FindControl("contentLink")).NavigateUrl = string.Format("DownloadFile.ashx?owner={0}&repository={1}&path={2}", requestOwner, requestRepository, entry.path);
                ((HyperLink)e.Row.FindControl("contentLink")).ToolTip = "Click to download a PowerShell digitally signed copy of this file";

                ((Label)e.Row.FindControl("Size")).Text = Utils.GetFileSizeString(entry.size);

                ((Label)e.Row.FindControl("Path")).Text = entry.path;
            }
            else if (entry.type == "repository")
            {
                ((Image)e.Row.FindControl("typeImage")).ImageUrl = "~/images/repository.jpg";
                ((HyperLink)e.Row.FindControl("contentLink")).NavigateUrl = string.Format("?owner={0}&repository={1}", requestOwner, entry.name);
                ((HyperLink)e.Row.FindControl("contentLink")).ToolTip = "Click to list repository contents";
            }
            else if (entry.type == "github")
            {
                ((Image)e.Row.FindControl("typeImage")).ImageUrl = "~/images/github.jpg";
            }

            ((HyperLink)e.Row.FindControl("contentLink")).Text = entry.name;
            ((Label)e.Row.FindControl("Type")).Text = entry.type;           
        }

        protected void gvFiles_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            if (e != null && e.NewPageIndex > -1)
            {
                gvFiles.PageIndex = e.NewPageIndex;

                gvFiles.DataSource = gitObjects;
                gvFiles.DataBind();
            }
        }

    }
}