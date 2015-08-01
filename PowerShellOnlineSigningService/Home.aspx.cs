using GitHubAPIClient;
using log4net;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

namespace PowerShellOnlineSigningService
{
    public partial class Home : System.Web.UI.Page
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private Requestor requestor = new Requestor();
        //private string folderURLPath = "<a href='?owner={0}&repository={1}&path={2}'>{2}</a>";
        //private string fileURLPath = "<a href='DownloadFile.ashx?owner={0}&repository={1}&path={2}' title='Click to download'>{2}</a>";
        private string folderImagePath = "~/images/folder.tiff";
        private string fileImagePath = "~/images/file.tiff";
        private string approvedExtensions = @"^.+\.((ps1)|(txt))$";

        //private List<GitContent> gitContents = new List<GitContent>();
        private List<GitObject> gitObjects = new List<GitObject>();

        protected void Page_Load(object sender, EventArgs e)
        {
            string contentOwner = Server.HtmlEncode(Request.QueryString["owner"]) ?? "Dscoduc"; //string.Empty;
            string contentRepo = Server.HtmlEncode(Request.QueryString["repository"]) ??  string.Empty;
            string contentPath = Server.HtmlEncode(Request.QueryString["path"]) ?? string.Empty;

            displayBreadcrumb();

            // displays username and servername
            displaySessionInfo();

            if (!Page.IsPostBack)
            {
                gvFiles_LoadData();
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
            NameValueCollection n = Request.QueryString;
            if (!n.HasKeys()) { return; }

            string o = Server.HtmlEncode(n["owner"]) ?? string.Empty;
            string r = Server.HtmlEncode(n["repository"]) ?? string.Empty;
            string p = Server.HtmlEncode(n["path"]) ?? string.Empty;

            string baseP = "<a href='{0}'>{1}</a>";

            string owner = string.Format(baseP, "?owner=" + o, o);
            string repository = string.Format(baseP, "?owner=" + o + "&repository=" + r, r);
            string path = string.Format(baseP, "?owner=" + o + "&repository=" + r + "&path=" + p, p);

            string breadcrumb = string.Empty;

            if (!string.IsNullOrEmpty(o))
            {
                breadcrumb = owner;

                if (!string.IsNullOrEmpty(r))
                {
                    breadcrumb = string.Format("{0} / {1}", owner, repository);

                    if (!string.IsNullOrEmpty(p))
                    {
                        string[] paths = p.Split(new char[] { '/' });

                        string fp = string.Join(" / ", paths);
                        breadcrumb = string.Format("{0} / {1} / {2}", owner, repository, fp);
                    }
                }
            }
            currentPath.InnerHtml = breadcrumb;
        }

        private void gvFiles_LoadData()
        {
            NameValueCollection n = Request.QueryString;
            if (!n.HasKeys()) { return; }

            string o = Server.HtmlEncode(n["owner"]) ?? string.Empty;
            string r = Server.HtmlEncode(n["repository"]) ?? string.Empty;
            string p = Server.HtmlEncode(n["path"]) ?? string.Empty;

            if (string.IsNullOrEmpty(r))
            {
                List<GitRepository> Repositories = null;
                try
                {
                    Repositories = GitHubClient.GetRepositories(o);
                }
                catch (Exception) { } // Continue - Repositories will be null and will exit gracefully

                if (null == Repositories || Repositories.Count < 1)
                {
                    log.InfoFormat("Notifying user no repositories were found for {0}", o);
                    //resultsInfo.InnerHtml = string.Format("<span class='error'>No repositories found for the repository owner {0}</span>", cachedOwner);

                    gitObjects.Add(new GitObject("No repositories found..."));

                }
                else
                {
                    foreach (GitRepository entry in Repositories)
                    {
                        gitObjects.Add(new GitObject(entry));
                    }
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
                        //gitContents.Add(entry);
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


            gvFiles.DataSource = gitObjects; // gitContents;
            gvFiles.DataBind();
        
        }

        protected void gvFiles_RowDataBound(object sender, GridViewRowEventArgs e)
        {

            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                //GitContent contentEntry = (GitContent)e.Row.DataItem;
                GitObject entry = (GitObject)e.Row.DataItem;
                if (entry != null)
                {

                    if (entry.type == "dir")
                    {
                        ((Image)e.Row.FindControl("typeImage")).ImageUrl = folderImagePath;
                        
                        ((HyperLink)e.Row.FindControl("contentLink")).NavigateUrl = string.Format("?owner={0}&repository={1}&path={2}", "Dscoduc", "PowerShellScripts", entry.path);

                        //((Label)e.Row.FindControl("Name")).Text = string.Format("<a href='?owner={0}&repository={1}&path={2}'>{2}</a>", "Dscoduc", "PowerShellScripts", contentEntry.path) ?? String.Empty;
                        ((Label)e.Row.FindControl("Size")).Text = string.Empty;

                        ((Label)e.Row.FindControl("Path")).Text = entry.path;
                    }
                    else if (entry.type == "file")
                    {
                        ((Image)e.Row.FindControl("typeImage")).ImageUrl = fileImagePath;

                        ((HyperLink)e.Row.FindControl("contentLink")).NavigateUrl = string.Format("DownloadFile.ashx?owner={0}&repository={1}&path={2}", "Dscoduc", "PowerShellScripts", entry.path);

                        //((Label)e.Row.FindControl("Name")).Text = string.Format(fileURLPath,"Dscoduc","PowerShellScripts", contentEntry.name);
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
            }
            
        }

        protected void gvFiles_RowCommand(object sender, CommandEventArgs e)
        {
            int index = Convert.ToInt32(e.CommandArgument);
            Label contentPath = (Label)gvFiles.Rows[index].FindControl("Path");
            Label contentName = (Label)gvFiles.Rows[index].FindControl("Name");
            Label contentType = (Label)gvFiles.Rows[index].FindControl("Type");


            if (e.CommandName == "HandleRequest")
            {
                string temp = contentType.Text;

                
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