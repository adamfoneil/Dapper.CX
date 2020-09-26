using SampleApp.RazorPages.Interfaces;
using System.Collections.Generic;

namespace SampleApp.RazorPages.Pages
{
    public partial class GridEditorModel : ICodeSample
    {
        public IEnumerable<CodeSample> Samples => new CodeSample[]
        {
            new CodeSample()
            {
                Title = "Code Behind",
                Url = "https://raw.githubusercontent.com/adamfoneil/Dapper.CX/code-samples/SampleApp.RazorPages/Pages/GridEditor.cshtml.cs"
            },
            new CodeSample()
            {
                Title = "Razor",
                Url = "https://raw.githubusercontent.com/adamfoneil/Dapper.CX/code-samples/SampleApp.RazorPages/Pages/GridEditor.cshtml",
                Language = "html"
            }
        };
    }
}
