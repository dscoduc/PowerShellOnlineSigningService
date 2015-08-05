using GitHubAPIClient;
using System;

namespace GitHubAPIClient
{
    public class GitObject : IDisposable
    {
        public string type { get; set; }
        public int size { get; set; }
        public string name { get; set; }
        public string path { get; set; }

        public GitObject()
        { }

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

        public void Dispose()
        {
            name = null;
            type = null;
            path = null;
            size = 0;
        }
    }
}
