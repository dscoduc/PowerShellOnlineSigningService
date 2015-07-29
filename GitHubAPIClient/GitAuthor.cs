
namespace GitHubAPIClient
{
    public class GitAuthor
    {
        public string name { get; set; }
        public string email { get; set; }

        public GitAuthor() { }
        public GitAuthor(string name, string email)
        {
            this.name = name;
            this.email = email;
        }
    }
}
