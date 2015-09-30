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
            populateBreadCrumb();
        }

        private void populateBreadCrumb()
        {
            PlaceHolder phBreadCrumbList = (PlaceHolder)Master.FindControl("crumbsPlaceHolder");
            string literal = "<ul class='breadcrumbList'>" +
                                 "<li><a href='Default.aspx'>Home</a></li>" +
                                 "<li><a href='UserGuide.aspx'>User Guide</a></li>" +
                             "</ul>";
            phBreadCrumbList.Controls.Add(new LiteralControl(literal));
        }
    }
}