using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GitHubAPIClient
{
    public class GitREADME
    {
        public string type { get; set; }
        public string encoding { get; set; }
        public int size { get; set; }
        public string name { get; set; }
        public string path { get; set; }
        public string content { get; set; }
        public string sha { get; set; }
        public string url { get; set; }
        public string git_url { get; set; }
        public string html_url { get; set; }
        public string download_url { get; set; }
        public ReadMe_Links _links { get; set; }

        /// <summary>
        /// Returns the base64 decoded content of the object 
        /// </summary>
        public string DecodedContent
        {
            get { return Utils.Base64Decode(this.content); }
        }
    }
    public class ReadMe_Links
    {
        public string git { get; set; }
        public string self { get; set; }
        public string html { get; set; }
    }
}
