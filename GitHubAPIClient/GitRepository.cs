
using System;
namespace GitHubAPIClient
{
    public class GitRepository : IComparable<GitRepository>
    {
        public int id { get; set; }
        public Owner owner { get; set; }
        public string name { get; set; }
        public string full_name { get; set; }
        public string description { get; set; }
        public bool @private { get; set; }
        public bool fork { get; set; }
        public string url { get; set; }
        public string html_url { get; set; }
        public string clone_url { get; set; }
        public string git_url { get; set; }
        public string ssh_url { get; set; }
        public string svn_url { get; set; }
        public string mirror_url { get; set; }
        public string homepage { get; set; }
        public object language { get; set; }
        public int forks_count { get; set; }
        public int stargazers_count { get; set; }
        public int watchers_count { get; set; }
        public int size { get; set; }
        public string default_branch { get; set; }
        public int open_issues_count { get; set; }
        public bool has_issues { get; set; }
        public bool has_wiki { get; set; }
        public bool has_pages { get; set; }
        public bool has_downloads { get; set; }
        public string pushed_at { get; set; }
        public string created_at { get; set; }
        public string updated_at { get; set; }
        public Permissions permissions { get; set; }

        public int CompareTo(GitRepository other)
        {
            // Default to type sort. [A to Z]
            return this.name.CompareTo(other.name);
        } 
    }
    public class Owner
    {
        public string login { get; set; }
        public int id { get; set; }
        public string avatar_url { get; set; }
        public string gravatar_id { get; set; }
        public string url { get; set; }
        public string html_url { get; set; }
        public string followers_url { get; set; }
        public string following_url { get; set; }
        public string gists_url { get; set; }
        public string starred_url { get; set; }
        public string subscriptions_url { get; set; }
        public string organizations_url { get; set; }
        public string repos_url { get; set; }
        public string events_url { get; set; }
        public string received_events_url { get; set; }
        public string type { get; set; }
        public bool site_admin { get; set; }
    }

    public class Permissions
    {
        public bool admin { get; set; }
        public bool push { get; set; }
        public bool pull { get; set; }
    }
}
