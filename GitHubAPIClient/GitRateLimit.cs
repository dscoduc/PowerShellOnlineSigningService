/*
 *  Class Generated from JSON using http://json2csharp.com/ 
 *
 *  Source JSON: https://api.github.com/repos/dscoduc/PowerShellScripts/git/trees/HEAD
 */
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace GitHubAPIClient
{
    public class GitRateLimit
    {
        [DataMember]
        public Resources resources { get; set; }
        [DataMember]
        public Rate rate { get; set; }
    }

    public class Core
    {
        public int limit { get; set; }
        public int remaining { get; set; }
        public int reset { get; set; }
    }

    public class Search
    {
        public int limit { get; set; }
        public int remaining { get; set; }
        public int reset { get; set; }
    }

    public class Resources
    {
        public Core core { get; set; }
        public Search search { get; set; }
    }

    public class Rate
    {
        public int limit { get; set; }
        public int remaining { get; set; }
        public int reset { get; set; }
    }

}
