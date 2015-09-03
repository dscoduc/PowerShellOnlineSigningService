using System;
using System.Web.UI.HtmlControls;

namespace PowerShellOnlineSigningService
{
    public partial class Oops : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            displayBreadcrumb();

        }

        private void displayBreadcrumb()
        {
            HtmlGenericControl site_breadcrumb = (HtmlGenericControl)Master.FindControl("site_breadcrumb");
            site_breadcrumb.Visible = true;
            site_breadcrumb.InnerHtml = "<a href='Default.aspx'>Home</a>";
        }

    }
}