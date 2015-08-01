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
            if (string.IsNullOrEmpty(filePath))
                return;

            try
            {
                FileInfo file = new FileInfo(filePath);
                if (file.Exists)
                {
                    file.Delete();
                    log.InfoFormat("Deleted {0}", filePath);
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

        /// <summary>
        /// <para>Returns the human-readable file size for an arbitrary, 64-bit file size</para>
        /// <para>The default format is "0.### XB", e.g. "4.2 KB" or "1.434 GB"</para>
        /// </summary>
        public static string GetFileSizeString(long i)
        {
            long absolute_i = (i < 0 ? -i : i);
            string suffix;
            double readable;

            // GB is enough for a VCS I think
            if (absolute_i >= 0x40000000)
            {
                suffix = "GB";
                readable = (i >> 20);
            }
            else if (absolute_i >= 0x100000)
            {
                suffix = "MB";
                readable = (i >> 10);
            }
            else if (absolute_i >= 0x400)
            {
                suffix = "kB";
                readable = i;
            }
            else
            {
                return i.ToString("0 B");
            }
            // Divide by 1024 to get fractional value
            readable = readable / 1024;
            return readable.ToString("0.### ") + suffix;
        }

    }

    //public class GitObject
    //{
    //    public string type { get; set; }
    //    public int size { get; set; }
    //    public string name { get; set; }
    //    public string path { get; set; }
        
    //    public GitObject(GitHubAPIClient.GitContent entry) 
    //    {
    //        this.name = entry.name;
    //        this.type = entry.type;
    //        this.size = entry.size;
    //        this.path = entry.path;
    //    }

    //    public GitObject(GitHubAPIClient.GitRepository entry)
    //    {
    //        this.name = entry.name;
    //        this.size = entry.size;
    //        this.type = "repository";
    //    }
    //}
}