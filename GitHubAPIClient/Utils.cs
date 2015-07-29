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
                log.InfoFormat("Cached data found for [{0}]", cacheKey);
                return cacheData;
            }
            else
            {
                log.InfoFormat("Cache data not found for [{0}]", cacheKey);
                return null;
            }
        }
    }
}
