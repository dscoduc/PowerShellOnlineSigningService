using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;

namespace GitHubAPIClient
{
    public static class GitHubClient
    {
        #region private declarations
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private static string github_root_url = ConfigurationManager.AppSettings["github_root_url"] ?? "api.github.com";
        #endregion // private declarations

        #region public functions
        /// <summary>
        /// Ex.
        ///     GitRateLimit rateLimit = GitHubClient.GetRateLimit();
        ///     Console.WriteLine("Rate Limit: {0}{1}Rate Remaining: {2}", rateLimit.rate.limit, Environment.NewLine, rateLimit.rate.remaining);
        /// </summary>
        /// <returns>Returns the RateLimit object</returns>
        public static GitRateLimit GetRateLimit()
        {
            log.Info("Requesting the Rate Limit from GitHub");

            HttpWebRequest request = buildWebRequest("https://api.github.com/rate_limit");

            string jsonResult = getResponse(request);

            GitRateLimit rateLimit = JsonConvert.DeserializeObject<GitRateLimit>(jsonResult);
            return rateLimit;
        }

        /// <summary>
        /// Ex.
        ///     if (GitHubClient.RateLimitExceeded())
        ///         Console.WriteLine("Rate limit hass exceeded allowed connections");
        /// </summary>
        /// <returns>Have you exceeded your allowed connections?</returns>
        public static bool RateLimitExceeded()
        {
            Rate rate = GetRateLimit().rate;
            return rate.remaining < 1;
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
            if (string.IsNullOrEmpty(contentPath) || string.IsNullOrEmpty(owner) || 
                string.IsNullOrEmpty(repository)) { throw new ArgumentNullException(); }

            string jsonResult = string.Empty;

            log.Info("Requesting the content of a file");

            // GET /repos/:owner/:repo/contents/:path
            string url = string.Format("https://{0}/repos/{1}/{2}/contents/{3}", github_root_url, owner, repository, contentPath);

            // Build request
            HttpWebRequest request = buildWebRequest(method.GET, url);

            try
            {
                // Submit request 
                jsonResult = getResponse(request);
            }
            catch (WebException wex)
            {
                if ((wex.Response).Headers["status"] == "404 Not Found")
                {
                    log.Info(wex.Message);
                    return null;
                }
                else
                {
                    log.Warn(wex);
                    throw;
                }
            }

            // convert json to object and return object
            GitContent content = JsonConvert.DeserializeObject<GitContent>(jsonResult);
            return content;
        }

        public static List<GitContent> GetContents(string owner, string repository, string contentPath)
        {
            if (string.IsNullOrEmpty(owner) || string.IsNullOrEmpty(repository)) { throw new ArgumentNullException(); }

            string jsonResult = string.Empty;

            // GET /repos/:owner/:repo/contents
            //string url = string.Format("https://api.github.com/repos/{0}/{1}/contents/{2}", owner, repository, contentPath);
            string url = string.Format("https://{0}/repos/{1}/{2}/contents/{3}", github_root_url, owner, repository, contentPath);

            // Build request
            HttpWebRequest request = buildWebRequest(method.GET, url);

            try
            {
                jsonResult = getResponse(request);
            }
            catch (WebException wex)
            {
                if ((wex.Response).Headers["status"] == "404 Not Found")
                {
                    log.Warn(wex.Message);
                    return null;
                }
                else
                {
                    log.Warn(wex);
                }
            }

            List<GitContent> contents = new List<GitContent>();

            // No obvious way to tell difference between a json result with a 
            // single entry result (non-array) or a json with multiple entries (array).  
            // This hack handles it for now...
            if (jsonResult.StartsWith("["))
            {
                contents = JsonConvert.DeserializeObject<List<GitContent>>(jsonResult);
            }
            else
            {
                GitContent content = JsonConvert.DeserializeObject<GitContent>(jsonResult);
                contents.Add(content);
            }

            // sort contents by type and then name
            contents.Sort();

            // sort by content type
            //contents.Sort((x,y) => x.type.CompareTo(y.type));
            
            // sort by content type, then by content name
            //List<GitContent> sortedContents = contents.OrderBy(o => o.type).ThenBy(o => o.name).ToList();

            log.InfoFormat("Returning {0} content items", contents.Count);
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
            if (string.IsNullOrEmpty(owner) || string.IsNullOrEmpty(repository)) { throw new ArgumentNullException(); }
            
            List<GitContent> contents = new List<GitContent>();
            string jsonResult = string.Empty;

            //TODO: Need to figure out how to loop through nested folders and grab the file info, then put them
            //      into a single array...

            // GET /repos/:owner/:repo/contents
            string url = string.Format("https://{0}/repos/{1}/{2}/contents", github_root_url, owner, repository);

            log.InfoFormat("Requesting a list of all files for {0}/{1}", owner, repository);

            // Build request
            HttpWebRequest request = buildWebRequest(method.GET, url);

            try
            {
                jsonResult = getResponse(request);
            }
            catch (WebException wex)
            {
                if ((wex.Response).Headers["status"] == "404 Not Found")
                {
                    log.Warn(wex.Message);
                    return null;
                }
                else
                {
                    log.Warn(wex);
                    throw;
                }
            }

            // No obvious way to tell difference between a json result with a 
            // single entry result (non-array) or a json with multiple entries (array).  
            // This hack handles it for now...
            if (jsonResult.StartsWith("["))
            {
                contents = JsonConvert.DeserializeObject<List<GitContent>>(jsonResult);
            }
            else
            {
                GitContent content = JsonConvert.DeserializeObject<GitContent>(jsonResult);
                contents.Add(content);
            }

            log.InfoFormat("Returning {0} content items", contents.Count);
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
            if (string.IsNullOrEmpty(owner) || string.IsNullOrEmpty(repository)) { throw new ArgumentNullException(); }

            GitContent content = GitHubClient.GetContent(owner, repository, contentPath);
            if (content == null)
            {
                log.Warn("No file matching the request was found in the Repository");
                return string.Empty;
            }
            return GetFileContents(content);
        }

        /// <summary>
        /// Ex.
        ///     Console.Write(GitHubClient.GetFileContents(Content));
        /// </summary>
        /// <param name="Content">Content object</param>
        /// <returns>Plain text output of the Base64 encoded contents of the requested file</returns>
        public static string GetFileContents(GitContent Content)
        {
            if (Content == null) { throw new ArgumentNullException(); }

            return Utils.Base64Decode(Content.content);
        }

        public static List<GitRepository> GetRepositories(string owner) 
        {
            if (string.IsNullOrEmpty(owner)) { throw new ArgumentNullException(); }

            List<GitRepository> contents = new List<GitRepository>();
            string jsonResult = string.Empty;

            // GET /users/:username/repos
            string url = string.Format("https://{0}/users/{1}/repos", github_root_url, owner);

            log.InfoFormat("Requesting a list of all repositories for {0}", owner);

            // Build request
            HttpWebRequest request = buildWebRequest(method.GET, url);

            try
            {
                jsonResult = getResponse(request);
            }
            catch (WebException wex)
            {
                if ((wex.Response).Headers["status"] == "404 Not Found")
                {
                    log.Warn(wex.Message);
                    return null;
                }
                else
                {
                    log.Warn(wex);
                    throw;
                }
            }

            // No obvious way to tell difference between a json result with a 
            // single entry result (non-array) or a json with multiple entries (array).  
            // This hack handles it for now...
            if (jsonResult.StartsWith("["))
            {
                contents = JsonConvert.DeserializeObject<List<GitRepository>>(jsonResult);
            }
            else
            {
                GitRepository content = JsonConvert.DeserializeObject<GitRepository>(jsonResult);
                contents.Add(content);
            }

            // sort repositories by name
            contents.Sort();

            log.InfoFormat("Returning {0} content items", contents.Count);
            return contents;
        
        }        
        #endregion // public functions

        #region private web functions
        /// <summary>
        /// Builds the web request with pre-set properties needed for GitHub
        /// using the default GET request method
        /// </summary>
        /// <param name="requestURL">The URL to perform the request against</param>
        /// <returns>Prepared request object</returns>
        private static HttpWebRequest buildWebRequest(string requestURL)
        {
            return buildWebRequest(method.GET, requestURL);
        }

        /// <summary>
        /// Builds the web request with pre-set properties needed for GitHub
        /// </summary>
        /// <param name="requestMethod">a http request method type (ex. GET, PUT, POST)</param>
        /// <param name="requestURL">The URL to perform the request against</param>
        /// <returns>Prepared request object</returns>
        private static HttpWebRequest buildWebRequest(method requestMethod, string requestURL)
        {
            if (string.IsNullOrEmpty(requestURL)) { throw new ArgumentNullException("Must provide request URL"); }

            log.InfoFormat("Request: {0} [{1}]", requestMethod, requestURL);

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(requestURL);
            request.Method = requestMethod.ToString();
            request.ContentType = "text/json";  // everything we're doing here is json based
            request.UserAgent = "curl"; //userAgent;  // GitHub requires userAgent be your username or repository
            request.Accept = "*/*";

            return request;
        }

        private static string getResponse(HttpWebRequest request)
        {
            HttpWebResponse getResponse;
            GitResponse cachedResponse;
            string jsonResponse = string.Empty;

            try
            {
                if ((request == null)) { throw new ArgumentNullException("An empty request object was passed"); }

                // check if the url is in cache
                object cacheData = Utils.GetCache(request.Address.AbsoluteUri);

                
                if (cacheData != null && request.Method == method.GET.ToString())
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
                            log.Info("Cached data is stale - updating memory cache with latest and greatest");
                            using (StreamReader streamReader = new StreamReader(getResponse.GetResponseStream()))
                            {
                                jsonResponse = streamReader.ReadToEnd();
                                log.DebugFormat("JSON response received:{0}{1}", Environment.NewLine, jsonResponse);

                                // add latest info to memory cache
                                Utils.AddCache(request.Address.AbsoluteUri, new GitResponse(jsonResponse, getResponse.Headers));

                                // send back the jsonResponse
                                return jsonResponse;
                            }
                        }
                    }
                    catch (WebException wex)
                    {
                        if (wex.Response != null && wex.Response.Headers.Get("Status") == "304 Not Modified")
                        {
                            log.Info("Cached data is still valid - new request not necessary");
                            return cachedResponse.JsonResponse;
                        }
                        else
                        {
                            log.Error(wex);
                            throw;
                        }
                    }
                    catch (Exception ex)
                    {
                        log.Error(ex);
                        throw;
                    }
                }
                else
                {
                    log.Info("Initiating a new request");
                    using (getResponse = (HttpWebResponse)request.GetResponse())
                    {
                        using (StreamReader streamReader = new StreamReader(getResponse.GetResponseStream()))
                        {
                            jsonResponse = streamReader.ReadToEnd();
                            log.DebugFormat("JSON response received:{0}{1}", Environment.NewLine, jsonResponse);

                            // add latest info to memory cache
                            log.Info("Updating memory cache with latest and greatest");
                            Utils.AddCache(request.Address.AbsoluteUri, new GitResponse(jsonResponse, getResponse.Headers));

                            return jsonResponse;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(ex);
                throw;
            }
        }

        /// <summary>
        /// HTTPWebRequest Methods
        /// </summary>
        private enum method
        {
            GET, POST, PUT, HEAD, DELETE
        }
        #endregion // private web functions

    }
}
