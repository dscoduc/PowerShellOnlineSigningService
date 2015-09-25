using log4net;
using System;
using System.Reflection;
using System.Web.UI.HtmlControls;

namespace PowerShellOnlineSigningService
{
    public partial class UserGuide : System.Web.UI.Page
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        protected void Page_Load(object sender, EventArgs e)
        {
            displayBreadcrumb();
        }

        private void displayBreadcrumb()
        {
            string urlTemplate = "<a href='{0}'>{1}</a>";

            string homeURL = string.Format(urlTemplate, "Default.aspx", "Home");
            string currentPage = string.Format(urlTemplate, "UserGuide.aspx", "User Guide");

            string breadcrumb = string.Format("{0} / {1}", homeURL, currentPage);


            HtmlGenericControl site_breadcrumb = (HtmlGenericControl)Master.FindControl("site_breadcrumb");
            site_breadcrumb.Visible = true;
            site_breadcrumb.InnerHtml = breadcrumb;
        }

    }
}