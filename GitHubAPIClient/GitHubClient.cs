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
        private static string auth_token = ConfigurationManager.AppSettings["auth_token"].ToString();
        private static string committer_name = ConfigurationManager.AppSettings["committer_name"].ToString();
        private static string committer_email = ConfigurationManager.AppSettings["committer_email"].ToString();
        private static string commit_message = ConfigurationManager.AppSettings["commit_message"].ToString();
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        #endregion // private declarations

        #region public functions
        /// <summary>
        /// Ex. 
        ///     GitUserData user = GitHubClient.GetUser();
        ///     Console.WriteLine("Name: {0}{1}Email: {2}", user.name, Environment.NewLine, user.email);
        /// </summary>
        /// <returns>Returns the authenticated user object</returns>
        public static GitUserData GetUser()
        {
            try
            {
                log.Info("Retrieving authenticated user");
                HttpWebRequest request = buildWebRequest("https://api.github.com/user");

                string jsonResult = getResponse(request);

                GitUserData userData = JsonConvert.DeserializeObject<GitUserData>(jsonResult);
                return userData;
            }
            catch (Exception ex)
            {
                log.Debug(ex);
                throw;
            }
        }

        /// <summary>
        /// Ex.
        ///     GitREADME readme = GitHubClient.GetReadme();
        ///     Console.WriteLine(readme.name);
        /// </summary>
        /// <returns>Returns the Repository default README object</returns>
        public static GitREADME GetReadme(string owner, string repository)
        {
            string jsonResult = string.Empty;

            if (string.IsNullOrEmpty(owner) || string.IsNullOrEmpty(repository)) { throw new ArgumentNullException(); }

            log.InfoFormat("Requesting the default README from {0}/{1}", owner, repository);

            // GET /repos/:owner/:repo/readme
            string url = string.Format("https://api.github.com/repos/{0}/{1}/readme", owner, repository);

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
            GitREADME readme = JsonConvert.DeserializeObject<GitREADME>(jsonResult);
            return readme;
        }

        /// <summary>
        /// Ex.
        ///     Console.WriteLine(GitHubClient.GetReadme().DecodedContent);
        /// </summary>
        /// <returns>Returns the text found in the Repository default README file</returns>
        public static string GetReadme_Content(string owner, string repository)
        {
            if (string.IsNullOrEmpty(owner) || string.IsNullOrEmpty(repository)) { throw new ArgumentNullException(); }
            return GetReadme(owner, repository).DecodedContent;
        }

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
            string url = string.Format("https://api.github.com/repos/{0}/{1}/contents/{2}", owner, repository, contentPath);

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

            // GET /repos/:owner/:repo/contents
            string url = string.Format("https://api.github.com/repos/{0}/{1}/contents", owner, repository);

            log.Info("Requesting a list of all files in the Repository");

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

            //    // No obvious way to tell difference between a json result with a 
            //    // single entry result (non-array) or a json with multiple entries (array).  
            //    // This hack handles it for now...
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
        ///     bool result = GitHubClient.UploadContent("c:\\temp\\myfile.ps1");
        /// </summary>
        /// <param name="sourceFile">The full file path to upload (ex. c:\temp\hello.txt)</param>
        /// <returns>Did upload succeeded?</returns>
        public static bool UploadContent(string sourceFile, string owner, string repository)
        {
            return UploadContent(sourceFile, owner, repository, string.Empty);
        }

        /// <summary>
        /// Ex. 
        ///     bool result = GitHubClient.UploadContent("c:\\temp\\myfile.ps1", "myfile.ps1");
        /// </summary>
        /// <param name="SourceFile">The full file path to upload (ex. c:\temp\hello.txt)</param>
        /// <param name="owner">The repository owner name</param>
        /// <param name="repository">The name of the repository</param>
        /// <param name="contentPath">The path of the file in the repository (ex. 'hello.txt' or 'folder/hello.txt')</param>
        /// <returns>Did the upload succeed?</returns>
        public static bool UploadContent(string SourceFile, string owner, string repository, string contentPath)
        {
            if (string.IsNullOrEmpty(SourceFile) || string.IsNullOrEmpty(owner) ||
                string.IsNullOrEmpty(repository)) { throw new ArgumentNullException(); }

            // if ContentPath isn't specified then assume root and use the source file name
            if (string.IsNullOrEmpty(contentPath)) { contentPath = Path.GetFileName(SourceFile); }

            log.InfoFormat("Processing a request to Create/Update {0} in {1}/{2}", contentPath, owner, repository);

            // See if we can find a file already in the hub
            GitContent content = GitHubClient.GetContent(owner, repository, contentPath);
            if (content != null)
            {
                log.Info("An existing file was found in the Repository");
                return UpdateFile(SourceFile, owner, repository, content);
            }

            log.Info("No existing file was found in the Repository");
            return CreateFile(SourceFile, owner, repository, contentPath);
        }

        /// <summary>
        /// Ex.
        ///     GitHubClient.DeleteContent("Hello.ps1")
        /// </summary>
        /// <param name="owner">The repository owner name</param>
        /// <param name="repository">The name of the repository</param>
        /// <param name="contentPath">The path of the file in the repository (ex. 'hello.txt' or 'folder/hello.txt')</param>
        /// <returns></returns>
        public static bool DeleteContent(string owner, string repository, string contentPath)
        {
            try
            {
                if (string.IsNullOrEmpty(contentPath) || string.IsNullOrEmpty(owner) ||
                    string.IsNullOrEmpty(repository)) { throw new ArgumentNullException(); }

                log.InfoFormat("Requesting the deletion of {0} in {1}/{2}", contentPath, owner, repository);

                // See if we can find a file already in the hub
                GitContent content = GitHubClient.GetContent(owner, repository, contentPath);
                if (content == null)
                    return false;            

                // DELETE /repos/:owner/:repo/contents/:path
                string url = string.Format("https://api.github.com/repos/{0}/{1}/contents/{2}", owner, repository, content.path);

                #region Build Request
                HttpWebRequest request = buildWebRequest(method.DELETE, url);
                using (StreamWriter streamWriter = new StreamWriter(request.GetRequestStream()))
                {
                    GitDeleteFile deleteFile = new GitDeleteFile();
                    deleteFile.message = commit_message;
                    //TODO: Replace author info with authenticated user info
                    //deleteFile.author = new GitAuthor("Chris B", "chris@dscoduc.com");
                    deleteFile.committer = new GitCommitter(committer_name, committer_email);
                    deleteFile.sha = content.sha;

                    // create json from object
                    string json = JsonConvert.SerializeObject(deleteFile, Formatting.None,
                        new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });

                    streamWriter.Write(json);
                    streamWriter.Flush();
                    streamWriter.Close();
                }
                #endregion // Build Request

                string jsonResult = getResponse(request);

                log.Info("File deleted from repository");
                return true;
            }
            catch (FileNotFoundException fex)
            {
                log.WarnFormat("File NOT deleted from Repository - {0}", fex.Message);
                return false;
            }
            catch (Exception ex)
            {
                log.Error(ex);
                throw;
            }
        }

        /// <summary>
        /// 
        /// Ex. GitHubClient.CreateFile("c:\\temp\\myfile.ps1", "myfile.ps1");
        /// </summary>
        /// <param name="sourceFile">The full file path to upload (ex. c:\temp\hello.txt)</param>
        /// <param name="contentPath">The path of the file in the repository (ex. 'hello.txt' or 'folder/hello.txt')</param>
        /// <returns>Did the creation succeed?</returns>
        public static bool CreateFile(string sourceFile, string owner, string repository, string contentPath)
        {
            if (string.IsNullOrEmpty(sourceFile) || string.IsNullOrEmpty(owner) ||
                string.IsNullOrEmpty(repository)) { throw new ArgumentNullException(); }

            log.InfoFormat("Requesting the creation of {0} in {1}/{2}", contentPath, owner, repository);

            // PUT /repos/:owner/:repo/contents/:path
            string url = string.Format("https://api.github.com/repos/{0}/{1}/contents/{2}", owner, repository, contentPath);

            #region Build Request
            HttpWebRequest request = buildWebRequest(method.PUT, url);
            using (StreamWriter streamWriter = new StreamWriter(request.GetRequestStream()))
            {
                GitCreateFile uploadFile = new GitCreateFile();
                uploadFile.message = commit_message;
                //TODO: Replace author info with authenticated user info
                //uploadFile.author = new GitAuthor("Chris B", "chris@dscoduc.com"); 
                uploadFile.committer = new GitCommitter(committer_name, committer_email);
                uploadFile.content = Utils.EncodeFile(sourceFile);

                // create json from object
                string json = JsonConvert.SerializeObject(uploadFile, Formatting.None, 
                    new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });

                streamWriter.Write(json);
                streamWriter.Flush();
                streamWriter.Close();
            }
            #endregion // Build Request

            try
            {
                string jsonResult = getResponse(request);
            }
            catch (Exception ex)
            {
                log.Error(ex);
                return false;
            }

            log.Info("File sucessfully created in Repository");
            return true;
        }

        /// <summary>
        /// 
        /// Ex. GitHubClient.UpdateFile("c:\\temp\\myfile.ps1", "myfile.ps1");
        /// </summary>
        /// <param name="sourceFile">The full file path to upload (ex. c:\temp\hello.txt)</param>
        /// <param name="contentPath">The path of the file in the repository (ex. 'hello.txt' or 'folder/hello.txt')</param>
        /// <returns>Did the update succeed?</returns>
        public static bool UpdateFile(string sourceFile, string owner, string repository, string contentPath)
        {
            try
            {
                if (string.IsNullOrEmpty(sourceFile) || string.IsNullOrEmpty(owner) ||
                    string.IsNullOrEmpty(repository)) { throw new ArgumentNullException(); }

                GitContent content = GitHubClient.GetContent(owner, repository, contentPath);
                if (content == null)
                {
                    log.Warn("Aborting Update due to missing file");
                    return false;
                }

                return UpdateFile(sourceFile, owner, repository, content);
            }
            catch (Exception ex)
            { 
                log.Error(ex);
                return false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sourceFile">The full file path to upload (ex. c:\temp\hello.txt)</param>
        /// <param name="content">Content object</param>
        /// <returns>True/False</returns>
        public static bool UpdateFile(string sourceFile, string owner, string repository, GitContent content)
        {
            try
            {
                if (string.IsNullOrEmpty(sourceFile) || string.IsNullOrEmpty(owner) ||
                    string.IsNullOrEmpty(repository)) { throw new ArgumentNullException(); }

                log.Info("Requesting an update to a file in the Repository");

                // PUT /repos/:owner/:repo/contents/:path
                string url = string.Format("https://api.github.com/repos/{0}/{1}/contents/{2}", owner, repository, content.path);

                #region Build Request
                HttpWebRequest request = buildWebRequest(method.PUT, url);
                using (StreamWriter streamWriter = new StreamWriter(request.GetRequestStream()))
                {
                    GitUpdateFile updateFile = new GitUpdateFile();
                    updateFile.message = commit_message;
                    //TODO: Replace author info with authenticated user info
                    //updateFile.author = new GitAuthor("Chris B", "chris@dscoduc.com");
                    updateFile.committer = new GitCommitter(committer_name, committer_email);
                    updateFile.content = Utils.EncodeFile(sourceFile);
                    updateFile.sha = content.sha;

                    // create json from object
                    string json = JsonConvert.SerializeObject(updateFile, Formatting.None, 
                        new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });

                    streamWriter.Write(json);
                    streamWriter.Flush();
                    streamWriter.Close();
                }
                #endregion // Build Request

                string jsonResult = getResponse(request);

                log.Info("File sucessfully updated in the Repository");
                return true;
            }
            catch (FileNotFoundException fex)
            {
                log.Warn(fex.Message);
                return false;
            }
            catch (Exception ex)
            {
                log.Error(ex);
                return false;
            }
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
            string url = string.Format("https://api.github.com/users/{0}/repos", owner);

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

            //    // No obvious way to tell difference between a json result with a 
            //    // single entry result (non-array) or a json with multiple entries (array).  
            //    // This hack handles it for now...
            if (jsonResult.StartsWith("["))
            {
                contents = JsonConvert.DeserializeObject<List<GitRepository>>(jsonResult);
            }
            else
            {
                GitRepository content = JsonConvert.DeserializeObject<GitRepository>(jsonResult);
                contents.Add(content);
            }

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

            if (!string.IsNullOrEmpty(auth_token))
                request.Headers.Add("authorization: token " + auth_token);

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
            catch (Exception)
            {

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
