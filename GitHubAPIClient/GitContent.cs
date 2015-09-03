using System;

namespace GitHubAPIClient
{
    public class GitContent : IComparable<GitContent>
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
        public Content_Links _links { get; set; }

        public string FileSize {
            get { return GetFileSizeString(size); }
        }

        /// <summary>
        /// <para>Returns the human-readable file size for an arbitrary, 64-bit file size</para>
        /// <para>The default format is "0.### XB", e.g. "4.2 KB" or "1.434 GB"</para>
        /// </summary>
        private static string GetFileSizeString(long i)
        {
            long absolute_i = (i < 0 ? -i : i);
            string suffix;
            double readable;

            // GB is enough for a VCS I think
            if (absolute_i >= 0x40000000)
            {
                suffix = "GB";
                readable = (i >> 20);
            }
            else if (absolute_i >= 0x100000)
            {
                suffix = "MB";
                readable = (i >> 10);
            }
            else if (absolute_i >= 0x400)
            {
                suffix = "kB";
                readable = i;
            }
            else
            {
                return i.ToString("0 B");
            }
            // Divide by 1024 to get fractional value
            readable = readable / 1024;
            return readable.ToString("0.### ") + suffix;
        }

        /// <summary>
        /// Returns the base64 decoded content of the object 
        /// </summary>
        public string DecodedContent
        {
            get 
            {
                byte[] base64EncodedBytes = System.Convert.FromBase64String(this.content);
                return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
            }
        }

        public int CompareTo(GitContent other)
        {
            // Alphabetic sort if type is equal. [A to Z]
            if (this.type == other.type)
            {
                return this.name.CompareTo(other.name);
            }

            // Default to type sort. [A to Z]
            return this.type.CompareTo(other.type);
        }   
    }

    public class Content_Links
    {
        public string git { get; set; }
        public string self { get; set; }
        public string html { get; set; }
    }
}
