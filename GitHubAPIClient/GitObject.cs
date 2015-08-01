using GitHubAPIClient;

namespace GitHubAPIClient
{
    public class GitObject
    {
        public string type { get; set; }
        public int size { get; set; }
        public string name { get; set; }
        public string path { get; set; }

        public GitObject(string message)
        {
            this.name = message;
        }

        public GitObject(GitContent entry) 
        {
            this.name = entry.name;
            this.type = entry.type;
            this.size = entry.size;
            this.path = entry.path;
        }

        public GitObject(GitRepository entry)
        {
            this.name = entry.name;
            this.size = entry.size;
            this.type = "repository";
        }
    }
}
