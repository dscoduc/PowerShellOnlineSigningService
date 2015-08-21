using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

namespace PowerShellOnlineSigningService
{
    public partial class Lost : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            displayBreadcrumb();
        }

        private void displayBreadcrumb()
        {
            string urlTemplate = "<a href='{0}'>{1}</a>";
            string homeURL = string.Format(urlTemplate, "Default.aspx", "Home");

            string breadcrumb = string.Format("{0}", homeURL);
            HtmlGenericControl site_breadcrumb = (HtmlGenericControl)Master.FindControl("site_breadcrumb");
            site_breadcrumb.Visible = true;
            site_breadcrumb.InnerHtml = breadcrumb;
        }
    }
}