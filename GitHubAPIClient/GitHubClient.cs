using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Net;
using System.Reflection;
using System.Runtime.Caching;

namespace GitHubAPIClient
{
    public static class GitHubClient
    {
        #region private declarations
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private static string github_root_url = (string)ConfigurationManager.AppSettings["github_root_url"] ?? "api.github.com";
        private static string auth_token = (string)ConfigurationManager.AppSettings["auth_token"] ?? null;
        private static string user_agent = (string)ConfigurationManager.AppSettings["user_agent"] ?? "curl/7.43.0";
        #endregion // private declarations

        #region public functions
        public static List<GitUserDetails> GetUsers(string searchString)
        {
            if (string.IsNullOrEmpty(searchString))
                throw new ArgumentNullException();

            // GET /search/users?q=:owner
            string url = string.Format("https://{0}/search/users?q={1}", github_root_url, searchString);

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);

            string jsonResponse = null;
            try
            {
                jsonResponse = getResponse(request);
            }
            catch (WebException wex)
            {
                log.Warn(wex);
                return null;
            }
            catch (Exception ex)
            {
                log.Error(ex);
                return null; 
            }

            GitUsers parsedResponse = JsonConvert.DeserializeObject<GitUsers>(jsonResponse);
            List<GitUserDetails> usersList = new List<GitUserDetails>();

            if(parsedResponse.items.Count == 0)
                return null;

            foreach (GitUser u in parsedResponse.items)
                usersList.Add(GetUserDetails(u));

            // sort the list before sending it onward
            usersList.Sort();

            log.DebugFormat("Returning {0} user entries", usersList.Count);
            return usersList;
        }

        /// <summary>
        /// Ex. 
        ///     GitContent content = GitHubClient.GetContent("hello.txt");
        ///     Console.WriteLine(content.name); 
        /// </summary>
        /// <param name="contentPath">The path of the file in the repository (ex. 'hello.txt' or 'folder/hello.txt')</param>
        /// <returns>The file object</returns>
        public static GitContent GetContent(string owner, string repository, string contentPath)
        {
            if (string.IsNullOrEmpty(contentPath) || string.IsNullOrEmpty(owner) || string.IsNullOrEmpty(repository)) 
                throw new ArgumentNullException();

            // GET /repos/:owner/:repo/contents/:path
            string url = string.Format("https://{0}/repos/{1}/{2}/contents/{3}", github_root_url, owner, repository, contentPath);

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);

            string jsonResponse = null;
            try 
            {
                jsonResponse = getResponse(request);
            }
            catch (WebException wex)
            {
                log.Warn(wex);
                return null;
            }
            catch (Exception ex)
            {
                log.Error(ex);
                return null;
            }

            return JsonConvert.DeserializeObject<GitContent>(jsonResponse);
        }

        public static GitUserDetails GetUserDetails(GitUser user)
        {
            if (null == user) 
                throw new ArgumentNullException();

            // GET /search/users
            string url = string.Format("https://{0}/users/{1}", github_root_url, user.login);

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);

            string jsonResponse = null;
            try
            {
                jsonResponse = getResponse(request);
            }
            catch (WebException wex)
            {
                log.Warn(wex);
                return null;
            }
            catch (Exception ex)
            {
                log.Error(ex);
                return null;
            }

            return JsonConvert.DeserializeObject<GitUserDetails>(jsonResponse);
        }

        public static List<GitContent> GetContents(string owner, string repository, string contentPath)
        {
            if (string.IsNullOrEmpty(owner) || string.IsNullOrEmpty(repository))
                throw new ArgumentNullException();

            // GET /repos/:owner/:repo/contents
            string url = string.Format("https://{0}/repos/{1}/{2}/contents/{3}", github_root_url, owner, repository, contentPath);

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);

            string jsonResponse = null;
            try
            {
                jsonResponse = getResponse(request);
            }
            catch (WebException wex)
            {
                log.Warn(wex);
                return null;
            }
            catch (Exception ex)
            {
                log.Error(ex);
                return null;
            }

            List<GitContent> contents = new List<GitContent>();

            // Json.Net can't tell difference between a json result with a 
            // single entry result (non-array) or a json with multiple entries (array).  
            // This hack handles it for now...
            if (jsonResponse.StartsWith("["))
            {
                contents = JsonConvert.DeserializeObject<List<GitContent>>(jsonResponse);
            }
            else
            {
                GitContent content = JsonConvert.DeserializeObject<GitContent>(jsonResponse);
                contents.Add(content);
            }

            // sort contents by type and then name
            contents.Sort();

            log.DebugFormat("Returning {0} content items", contents.Count);
            return contents;
        }

        /// <summary>
        /// Ex. 
        ///     List<GitContent> contents = GitHubClient.GetContents();
        ///     foreach (GitContent entry in contents)
        ///         Console.WriteLine("{0} [{1}] [{2}]", entry.name, entry.FileSize, entry.download_url);
        /// </summary>
        /// <returns>An array of all objects in the Repository</returns>
        public static List<GitContent> GetContents(string owner, string repository)
        {
            if (string.IsNullOrEmpty(owner) || string.IsNullOrEmpty(repository))
                throw new ArgumentNullException();

            // GET /repos/:owner/:repo/contents
            string url = string.Format("https://{0}/repos/{1}/{2}/contents", github_root_url, owner, repository);

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);

            string jsonResponse = null;
            try
            {
                jsonResponse = getResponse(request);
            }
            catch (WebException wex)
            {
                log.Warn(wex);
                return null;
            }
            catch (Exception ex)
            {
                log.Error(ex);
                return null;
            }

            List<GitContent> contents = new List<GitContent>();
            // Json.Net can't tell difference between a json result with a 
            // single entry result (non-array) or a json with multiple entries (array).  
            // This hack handles it for now...
            if (jsonResponse.StartsWith("["))
            {
                contents = JsonConvert.DeserializeObject<List<GitContent>>(jsonResponse);
            }
            else
            {
                GitContent content = JsonConvert.DeserializeObject<GitContent>(jsonResponse);
                contents.Add(content);
            }

            log.DebugFormat("Returning {0} content items", contents.Count);
            return contents;
        }

        /// <summary>
        /// Ex.
        ///     Console.WriteLine(GitHubClient.GetFileContents("hello.txt"));
        /// </summary>
        /// <param name="contentPath">The path of the file in the repository (ex. 'hello.txt' or 'folder/hello.txt')</param>
        /// <returns>Plain text output of the Base64 encoded contents of the requested file</returns>
        public static string GetFileContents(string owner, string repository, string contentPath)
        {
            if (string.IsNullOrEmpty(owner) || string.IsNullOrEmpty(repository) || string.IsNullOrEmpty(contentPath)) 
                throw new ArgumentNullException();

            GitContent jsonResponse = GitHubClient.GetContent(owner, repository, contentPath);
            if (null == jsonResponse)
            {
                log.DebugFormat("No file matching '{0}' was found in the Repository", contentPath);
                return null;
            }

            // decode from base64
            //byte[] base64EncodedBytes = System.Convert.FromBase64String(jsonResponse.content);

            //return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
            return jsonResponse.DecodedContent;
        }

        public static List<GitRepository> GetRepositories(string owner) 
        {
            if (string.IsNullOrEmpty(owner)) 
                throw new ArgumentNullException();

            // GET /users/:username/repos
            string url = string.Format("https://{0}/users/{1}/repos", github_root_url, owner);

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);

            string jsonResponse = null;
            try
            {
                jsonResponse = getResponse(request);
            }
            catch (WebException wex)
            {
                log.Warn(wex);
                return null;
            }
            catch (Exception ex)
            {
                log.Error(ex);
                return null;
            }

            List<GitRepository> contents = new List<GitRepository>();
            // Json.Net can't tell difference between a json result with a 
            // single entry result (non-array) or a json with multiple entries (array).  
            // This hack handles it for now...
            if (jsonResponse.StartsWith("["))
                contents = JsonConvert.DeserializeObject<List<GitRepository>>(jsonResponse);
            else
                contents.Add(JsonConvert.DeserializeObject<GitRepository>(jsonResponse));

            // sort repositories by name
            contents.Sort();

            log.DebugFormat("Returning {0} content items", contents.Count);
            return contents;
        
        }        
        #endregion // public functions

        #region private web functions
        private static string getResponse(HttpWebRequest request)
        {
            if ((null == request)) { throw new ArgumentNullException("An empty request object was passed"); }

            HttpWebResponse getResponse;
            GitResponse cachedResponse;
            string jsonResponse = string.Empty;

            // add default request options
            request.Accept = "*/*";
            request.Method = "GET";
            request.ContentType = "application/json";
            request.UserAgent = user_agent;             // NOTE: GitHub requires userAgent be your username or repository

            if (!string.IsNullOrEmpty(auth_token))      // NOTE: Auth token is optional on public Github
                request.Headers.Add("authorization: token " + auth_token);

            // check if the url is in cache
            object cacheData = GetCache(request.Address.AbsoluteUri);
                
            if (null != cacheData && request.Method == "GET")
            {
                // data found in cache, load it up
                cachedResponse = (GitResponse)cacheData;

                // grab etag and add into request to be made to see if it's expired
                request.Headers.Add("If-None-Match", cachedResponse.GetETAG);

                // perform the request to see if the response would be different
                try
                {
                    // if data is stale then a HTTP 304 response will drop this into an WebException
                    // else it will contain updated information
                    using (getResponse = (HttpWebResponse)request.GetResponse())
                    {
                        log.DebugFormat("Updating cache for [{0}]", request.Address.AbsoluteUri);
                        using (StreamReader streamReader = new StreamReader(getResponse.GetResponseStream()))
                        {
                            jsonResponse = streamReader.ReadToEnd();
                            log.DebugFormat("JSON response received:{0}{1}", Environment.NewLine, jsonResponse);

                            // add latest info to memory cache
                            AddCache(request.Address.AbsoluteUri, new GitResponse(jsonResponse, getResponse.Headers));

                            // send back the jsonResponse
                            return jsonResponse;
                        }
                    }
                }
                catch (WebException wex)
                {
                    if (wex.Response != null && wex.Response.Headers.Get("Status") == "304 Not Modified")
                    {
                        log.Debug("Cached data is still valid - new request not necessary");
                        return cachedResponse.JsonResponse;
                    }
                    else { throw; }
                }
                catch (Exception) { throw; }
            }
            else
            {
                using (getResponse = (HttpWebResponse)request.GetResponse())
                {
                    using (StreamReader streamReader = new StreamReader(getResponse.GetResponseStream()))
                    {
                        jsonResponse = streamReader.ReadToEnd();
                        log.DebugFormat("JSON response received:{0}{1}", Environment.NewLine, jsonResponse);

                        if (!string.IsNullOrEmpty(getResponse.GetResponseHeader("ETag")))
                        {
                            // add latest info to memory cache
                            log.DebugFormat("Updating cache for [{0}]", request.Address.AbsoluteUri);
                            AddCache(request.Address.AbsoluteUri, new GitResponse(jsonResponse, getResponse.Headers));
                        }
                        else
                        {
                            log.Debug("Not caching due to an absence of ETag data");
                        }

                        return jsonResponse;
                    }
                }
            }
        }
        #endregion // private web functions

        #region utility functions
        public static void AddCache(string cacheKey, object cacheData)
        {
            string cache_timeout_min = (string)ConfigurationManager.AppSettings["cache_timeout_min"] ?? "30";

            ObjectCache cache = MemoryCache.Default;
            CacheItemPolicy policy = new CacheItemPolicy { 
                SlidingExpiration = TimeSpan.FromMinutes(Convert.ToDouble(cache_timeout_min)) 
            };

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
        #endregion // utility functions
    }
}
