
namespace GitHubAPIClient
{
    public class GitDeleteFile
    {
        public string message { get; set; }
        public GitCommitter committer { get; set; }
        public GitAuthor author { get; set; }
        public string sha { get; set; }
        public string branch { get; set; }

        public GitDeleteFile() { }
    }
}
