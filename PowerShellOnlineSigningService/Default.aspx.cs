using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

namespace PowerShellOnlineSigningService
{
    public partial class Default : System.Web.UI.Page
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        protected void Page_Load(object sender, EventArgs e)
        {
            populateBreadCrumb();
        }

        private void populateBreadCrumb()
        {
            PlaceHolder phBreadCrumbList = (PlaceHolder)Master.FindControl("crumbsPlaceHolder");
            string literal = "<ul class='breadcrumbList'>" +
                                 "<li><a href='Default.aspx'>Home</a></li>" +
                                 "<li><a href='Default.aspx'>Home</a></li>" + // needed to have 'Home' after home image
                             "</ul>";
            phBreadCrumbList.Controls.Add(new LiteralControl(literal));
        }

        protected void btnSearch_Click(object sender, EventArgs e)
        {
            TextBox tbSearch = (TextBox)Master.FindControl("cphBody").FindControl("tbSearch");
            if (string.IsNullOrEmpty(tbSearch.Text))
                Response.Redirect("~/Search.aspx?s=a", true);

            Response.Redirect(string.Format("~/Search.aspx?s={0}", tbSearch.Text), true);
        }
    }
}