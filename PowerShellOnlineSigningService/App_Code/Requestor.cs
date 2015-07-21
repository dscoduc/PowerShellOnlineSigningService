using log4net;
using System;
using System.Reflection;
using System.Web;

namespace PowerShellOnlineSigningService
{
    public class Requestor : IDisposable
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        #region Public Properties
        public string IIS_Auth_Name;
        public string UserDomain;
        public string samAccountName;
        public string IP_Address;
        #endregion // Public Properties

        public Requestor()
        {
            this.IP_Address = HttpContext.Current.Request.UserHostAddress;

            if (HttpContext.Current.User.Identity.IsAuthenticated)
            {
                this.IIS_Auth_Name = HttpContext.Current.User.Identity.Name;
                this.samAccountName = this.IIS_Auth_Name.Remove(0, this.IIS_Auth_Name.LastIndexOf(@"\") + 1);
                this.UserDomain = this.IIS_Auth_Name.Remove(this.IIS_Auth_Name.LastIndexOf(@"\"));

                log.DebugFormat("Authenticated as {0} from {1}", this.IIS_Auth_Name, IP_Address);
            }
            else
            {
                this.IIS_Auth_Name = "Anonymous User";
                this.samAccountName = this.IIS_Auth_Name;
                log.WarnFormat("Authenticated as {0} from {1}", this.IIS_Auth_Name, IP_Address);
            }

        }

        public void Dispose()
        {
            IIS_Auth_Name = string.Empty;
            samAccountName = string.Empty;
            IP_Address = string.Empty;
            UserDomain = string.Empty;
        }
    }
}