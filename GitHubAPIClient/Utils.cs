using log4net;
using System;
using System.IO;
using System.Reflection;
using System.Runtime.Caching;

namespace GitHubAPIClient
{
    public static class Utils
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public static string EncodeFile(string FilePath)
        {
            string plainText = File.ReadAllText(FilePath);
            return Base64Encode(plainText);
        }

        public static string Base64Encode(string plainText)
        {
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(bytes);
        }

        public static string Base64Decode(string base64EncodedData)
        {
            var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
            return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
        }

        public static void AddCache(string cacheKey, object cacheData)
        {
            ObjectCache cache = MemoryCache.Default;
            CacheItemPolicy policy = new CacheItemPolicy { SlidingExpiration = TimeSpan.FromHours(2) };
            cache.Add(cacheKey, cacheData, policy);
        }

        public static object GetCache(string cacheKey)
        {
            ObjectCache memoryCache = MemoryCache.Default;
            object cacheData = (object)memoryCache.Get(cacheKey);
            if (cacheData != null)
            {
                log.DebugFormat("Cached data found for [{0}]", cacheKey);
                return cacheData;
            }
            else
            {
                log.DebugFormat("Cache data not found for [{0}]", cacheKey);
                return null;
            }
        }

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
}
