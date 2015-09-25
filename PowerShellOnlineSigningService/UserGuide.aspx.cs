using log4net;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

namespace PowerShellOnlineSigningService
{
    public partial class UserGuide : System.Web.UI.Page
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        protected void Page_Load(object sender, EventArgs e)
        {
            //displayBreadcrumb();
            populateBreadCrumb();
        }

        private void populateBreadCrumb()
        {
            PlaceHolder phBreadCrumbList = (PlaceHolder)Master.FindControl("crumbsPlaceHolder");
            phBreadCrumbList.Controls.Add(new LiteralControl("<ul class='breadcrumbList'>"));

            List<string> items = new List<string>();

            items.Add("<li><a href='Default.aspx'>Home</a></li>");
            items.Add("<li>User Guide</li>");

            foreach (var item in items)
                phBreadCrumbList.Controls.Add(new LiteralControl(item));

            phBreadCrumbList.Controls.Add(new LiteralControl("</ul>"));
        }

    }
}