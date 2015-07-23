using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using log4net;
using System.Reflection;
using System.Text;

namespace PowerShellOnlineSigningService
{
    public static class WebUtils
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public static void DeleteFile(string filePath)
        {
            try
            {
                FileInfo file = new FileInfo(filePath);
                if (file.Exists)
                {
                    file.Delete();
                    log.DebugFormat("Deleted {0}", filePath);
                }
                else
                {
                    log.WarnFormat("Unable to delete {0} - file not found", filePath);
                }
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }

    }
}