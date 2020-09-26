using SampleApp.RazorPages.Interfaces;
using System.Collections.Generic;

namespace SampleApp.RazorPages.Models
{
    public class GitHubSourceRequest
    {
        public string Url { get; set; }
        public IEnumerable<CodeSample.Link> Links { get; set; }
    }
}
