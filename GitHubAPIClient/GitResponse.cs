using System.Net;

namespace GitHubAPIClient
{
    public class GitResponse
    {
        public string JsonResponse;
        public WebHeaderCollection ResponseHeaders;

        public GitResponse() { }
        public GitResponse(string JsonResponse, WebHeaderCollection ResponseHeaders)
        {
            this.JsonResponse = JsonResponse;
            this.ResponseHeaders = ResponseHeaders;
        }

        public string GetETAG 
        {
            get { return this.ResponseHeaders.Get("ETag");}
        }

        public string GetStatus
        {
            get { return this.ResponseHeaders.Get("Status"); }
        }

    }
}
