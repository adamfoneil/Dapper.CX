using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SampleApp.RazorPages.Interfaces
{
    /// <summary>
    /// allows a page to define one or more code samples from GitHub to import into a Razor page for tutorial purposes
    /// </summary>
    public interface ICodeSample
    {
        IEnumerable<CodeSample> Samples { get; }
    }

    public class CodeSample
    {
        public string Title { get; set; }
        public string Url { get; set; }
        public string Comments { get; set; }
        public string Language { get; set; } = "csharp";
        public string ImportElementId { get; set; }

        public string GetFilename() => Path.GetFileName(Url);

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
    }
}
