using System;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;

namespace PowerShellOnlineSigningService
{
    public partial class Main : System.Web.UI.MasterPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            displaySessionInfo();
        }

        private void displaySessionInfo()
        {
            string IIS_Auth_Name = string.Empty;
            string samAccountName = "Anonymous User";

            if (HttpContext.Current.User.Identity.IsAuthenticated)
            {
                IIS_Auth_Name = HttpContext.Current.User.Identity.Name;
                samAccountName = IIS_Auth_Name.Remove(0, IIS_Auth_Name.LastIndexOf(@"\") + 1);
            }

            HtmlGenericControl userInfo = (HtmlGenericControl)Page.Master.FindControl("userInfo");
            if (userInfo != null)
                userInfo.InnerText = string.Format("Welcome {0}", samAccountName);

            HtmlGenericControl serverInfo = (HtmlGenericControl)Page.Master.FindControl("serverInfo");
            if (serverInfo != null)
                serverInfo.InnerText = System.Environment.MachineName.ToLower();
        }
    }
}