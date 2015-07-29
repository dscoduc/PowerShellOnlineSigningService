
namespace GitHubAPIClient
{
    public class GitUpdateFile
    {
        public string message { get; set; }
        public GitCommitter committer { get; set; }
        public GitAuthor author { get; set; }
        public string content { get; set; }
        public string sha { get; set; }
        public string branch { get; set; }

        public GitUpdateFile() { }
        public GitUpdateFile(string message, string content) 
        {
            this.message = message;
            this.content = content;
        }
    }
}
