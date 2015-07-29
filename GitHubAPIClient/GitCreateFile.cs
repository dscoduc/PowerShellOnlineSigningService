
namespace GitHubAPIClient
{
    public class GitCreateFile
    {
        //{
        //  "message": "my commit message",
        //  "committer": {
        //    "name": "Scott Chacon",
        //    "email": "schacon@gmail.com"
        //  },
        //  "author": {
        //    "name": "Scott Chacon",
        //    "email": "schacon@gmail.com"
        //  },
        //  "content": "bXkgdXBkYXRlZCBmaWxlIGNvbnRlbnRz",
        //}

        public string message { get; set; }
        public GitCommitter committer { get; set; }
        public GitAuthor author { get; set; }
        public string content { get; set; }

        public GitCreateFile() { }
        public GitCreateFile(string message, string content) 
        {
            this.message = message;
            this.content = content;
        }
    }
}
