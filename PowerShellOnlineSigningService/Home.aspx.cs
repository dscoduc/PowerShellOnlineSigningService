using GitHubAPIClient;
using log4net;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

namespace PowerShellOnlineSigningService
{
    public partial class Home : System.Web.UI.Page
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private Requestor requestor = new Requestor();
        private string folderImagePath = "~/images/folder.tiff";
        private string fileImagePath = "~/images/file.tiff";
        private string approvedExtensions = @"^.+\.((ps1)|(txt))$";
        private List<GitObject> gitObjects = new List<GitObject>();

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Page.IsPostBack)
            {
                string redirectURL = "home.aspx";
                if(string.IsNullOrEmpty(tbOwner.Text))
                {
                    Response.Redirect(redirectURL);
                }
                else
                {
                    Response.Redirect(string.Format("{0}?owner={1}", redirectURL, tbOwner.Text));
                }
            }
            else
            {
                displaySessionInfo();

                gvFiles_LoadData();

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

            string o = Server.HtmlEncode(Request.QueryString["owner"]) ?? string.Empty;
            string r = Server.HtmlEncode(Request.QueryString["repository"]) ?? string.Empty;
            string p = Server.HtmlEncode(Request.QueryString["path"]) ?? string.Empty;

            string urlTemplate = "<a href='{0}'>{1}</a>";
            
            string pathURL = string.Format(urlTemplate, "?owner=" + o + "&repository=" + r + "&path=" + p, p);

            string breadcrumb = string.Empty;

            if (!string.IsNullOrEmpty(o))
            {
                string ownerURL = string.Format(urlTemplate, "?owner=" + o, o);
                breadcrumb = ownerURL;

                if (!string.IsNullOrEmpty(r))
                {
                    string repositoryURL = string.Format(urlTemplate, "?owner=" + o + "&repository=" + r, r);
                    breadcrumb = string.Format("{0} / {1}", ownerURL, repositoryURL);

                    if (!string.IsNullOrEmpty(p))
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

            string o = Server.HtmlEncode(Request.QueryString["owner"]);
            string r = Server.HtmlEncode(Request.QueryString["repository"]) ?? string.Empty;
            string p = Server.HtmlEncode(Request.QueryString["path"]) ?? string.Empty;

            string urlTemplate = "<a href='?owner=" + o + "&repository=" + r + "&path={0}'>{1}</a>";
            string breadcrumb = string.Empty;
            ArrayList al = new ArrayList();

            if (p.Contains("/"))
            {
                do
                {
                    string folderName = p.Substring(p.LastIndexOf("/") + 1);
                    string link = string.Format(urlTemplate, p, folderName);

                    al.Add(link);

                    // advance through the paths
                    p = p.Substring(0, p.LastIndexOf("/"));

                } while (p.Contains("/"));
            }

            al.Add(string.Format(urlTemplate, p, p));

            for (int x = al.Count - 1; x >= 0; x--)
            {
                breadcrumb = string.Format("{0} / {1}", breadcrumb, al[x]);
            }

            return breadcrumb.Substring(3);
        }

        private void gvFiles_LoadData()
        {
            string o = Server.HtmlEncode(Request.QueryString["owner"]);
            string r = Server.HtmlEncode(Request.QueryString["repository"]) ?? string.Empty;
            string p = Server.HtmlEncode(Request.QueryString["path"]) ?? string.Empty;

            if (string.IsNullOrEmpty(o)) 
            {
                gitObjects.Add(new GitObject("Specify a GitHub account to view the available repositories..."));                
            }
            else if (string.IsNullOrEmpty(r))
            {
                List<GitRepository> Repositories = null;
                
                try { Repositories = GitHubClient.GetRepositories(o); }
                catch (Exception) { } // Continue - Repositories will be null and will exit gracefully

                if (null == Repositories || Repositories.Count < 1)
                {
                    log.InfoFormat("Notifying user no repositories were found for {0}", o);
                    gitObjects.Add(new GitObject("No repositories found..."));
                }
                else
                {
                    foreach (GitRepository entry in Repositories)
                        gitObjects.Add(new GitObject(entry));
                }
            }
            else
            {
                List<GitContent> unfilteredContents = GitHubClient.GetContents(o, r, p);

                foreach (GitContent entry in unfilteredContents)
                {
                    GitObject m = new GitObject(entry);
                    if (entry.type == "dir")
                    {
                        m.name = entry.name;
                        m.path = entry.path;
                        m.type = entry.type;
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
                        }
                    }
                    gitObjects.Add(m);
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
                ((Image)e.Row.FindControl("typeImage")).ImageUrl = folderImagePath;
                        
                ((HyperLink)e.Row.FindControl("contentLink")).NavigateUrl = string.Format("?owner={0}&repository={1}&path={2}", "Dscoduc", "PowerShellScripts", entry.path);

                ((Label)e.Row.FindControl("Size")).Text = string.Empty;

                ((Label)e.Row.FindControl("Path")).Text = entry.path;
            }
            else if (entry.type == "file")
            {
                ((Image)e.Row.FindControl("typeImage")).ImageUrl = fileImagePath;

                ((HyperLink)e.Row.FindControl("contentLink")).NavigateUrl = string.Format("DownloadFile.ashx?owner={0}&repository={1}&path={2}", "Dscoduc", "PowerShellScripts", entry.path);

                ((Label)e.Row.FindControl("Size")).Text = WebUtils.GetFileSizeString(entry.size);

                ((Label)e.Row.FindControl("Path")).Text = entry.path;
            }
            else if (entry.type == "repository")
            {
                ((HyperLink)e.Row.FindControl("contentLink")).NavigateUrl = string.Format("?owner={0}&repository={1}", "Dscoduc", "PowerShellScripts");
            }                    

            ((HyperLink)e.Row.FindControl("contentLink")).Text = entry.name;
            ((Label)e.Row.FindControl("Type")).Text = entry.type;           
        }

        protected void gvFiles_RowCommand(object sender, CommandEventArgs e)
        {
            int index = Convert.ToInt32(e.CommandArgument);
            Label contentPath = (Label)gvFiles.Rows[index].FindControl("Path");
            Label contentName = (Label)gvFiles.Rows[index].FindControl("Name");
            Label contentType = (Label)gvFiles.Rows[index].FindControl("Type");

            if (e.CommandName == "HandleRequest")
            {
                // placeholder
            }            
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