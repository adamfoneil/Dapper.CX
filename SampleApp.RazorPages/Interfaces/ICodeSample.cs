using SampleApp.RazorPages.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Web;

namespace SampleApp.RazorPages.Interfaces
{
    public enum LinkType
    {
        /// <summary>
        /// a CodeSample.ElementId
        /// </summary>
        CodeSample,
        /// <summary>
        /// a Tooltip element Id
        /// </summary>
        Tooltip
    }

    /// <summary>
    /// allows a page to define one or more code samples from GitHub to import into a Razor page for tutorial purposes
    /// </summary>
    public interface ICodeSample
    {
        IEnumerable<CodeSample> Samples { get; }
    }

    public class CodeSample
    {
        /// <summary>
        /// used as target for links
        /// </summary>
        public string ElementId { get; set; }

        /// <summary>
        /// accordion card header content
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// this should always be a "raw" URL to a source file on GitHub, e.g. "https://raw.githubusercontent.com...
        /// </summary>
        public string Url { get; set; }        
        public string Language { get; set; } = "csharp";

        /// <summary>
        /// element Id on the razor page that will be imported in the accordion card body, used
        /// for adding explanator content for a code sample
        /// </summary>
        public string ImportElementId { get; set; }

        public IEnumerable<Link> Links { get; set; }        

        public string GetFilename() => Path.GetFileName(Url);

        public string SourceRequestJson
        {
            get => HttpUtility.HtmlEncode(JsonSerializer.Serialize(new GitHubSourceRequest()
            {
                Url = Url,
                Links = Links
            }));                
        }

        // converts a "raw" GitHub URL to the regular URL
        public string GetFullUrl()
        {
            const int repoNameFolder = 3;
            var rawUri = new Uri(Url);
            var folders = rawUri.LocalPath.Split('/').ToList();
            folders.Insert(repoNameFolder, "blob");
            string newPath = string.Join("/", folders);
            return rawUri.AbsoluteUri.Replace(rawUri.Authority, "github.com").Replace(rawUri.LocalPath, newPath);
        }

        public class Link
        {
            public Link()
            {
            }

            public Link(LinkType type, string text, string target)
            {
                Type = type;
                Text = text;
                Target = target;
            }

            public LinkType Type { get; set; }
            public string Text { get; set; }
            public string Target { get; set; }

            public string CssClass 
            {  
                get =>
                    (Type == LinkType.Tooltip) ? "sample-tooltip" :
                    (Type == LinkType.CodeSample) ? "code-sample" :
                    throw new Exception($"Unknown link type {Type}");
            }
        }
    }
}
