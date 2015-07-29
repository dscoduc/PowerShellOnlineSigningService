
namespace GitHubAPIClient
{
    public class GitCommitter
    {
        public string name { get; set; }
        public string email { get; set; }

        public GitCommitter() { }
        public GitCommitter(string name, string email)
        {
            this.name = name;
            this.email = email;
        }
    }
}
