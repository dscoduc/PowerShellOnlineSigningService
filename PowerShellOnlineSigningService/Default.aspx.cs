using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

namespace PowerShellOnlineSigningService
{
    public partial class Default : System.Web.UI.Page
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

        protected void btnSearch_Click(object sender, EventArgs e)
        {
            TextBox tbSearch = (TextBox)Master.FindControl("cphBody").FindControl("tbSearch");
            if (string.IsNullOrEmpty(tbSearch.Text))
                Response.Redirect("~/SearchUsers.aspx?s=a", true);

            Response.Redirect(string.Format("~/SearchUsers.aspx?s={0}", tbSearch.Text), true);
        }
    }
}