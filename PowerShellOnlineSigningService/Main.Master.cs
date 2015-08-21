using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

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
            if (HttpContext.Current.User.Identity.IsAuthenticated)
            {
                string IIS_Auth_Name = HttpContext.Current.User.Identity.Name;
                string samAccountName = IIS_Auth_Name.Remove(0, IIS_Auth_Name.LastIndexOf(@"\") + 1);

                HtmlGenericControl userInfo = (HtmlGenericControl)Page.Master.FindControl("userInfo");
                if (userInfo != null)
                    userInfo.InnerText = string.Format("Welcome {0}", samAccountName);
            }

            HtmlGenericControl serverInfo = (HtmlGenericControl)Page.Master.FindControl("serverInfo");
            if (serverInfo != null)
                serverInfo.InnerText = System.Environment.MachineName.ToLower();
        }
    }
}