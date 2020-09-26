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
                Url = "https://raw.githubusercontent.com/adamfoneil/Dapper.CX/code-samples/SampleApp.RazorPages/Pages/GridEditor.cshtml.cs",
                Links = new CodeSample.Link[]
                {
                    new CodeSample.Link(LinkType.Tooltip, "OnPostSaveItemAsync", "SaveItem"),
                    new CodeSample.Link(LinkType.Tooltip, "OnPostDeleteItemAsync", "DeleteItem")
                }
            },
            new CodeSample()
            {
                Title = "Razor",
                Url = "https://raw.githubusercontent.com/adamfoneil/Dapper.CX/code-samples/SampleApp.RazorPages/Pages/GridEditor.cshtml",
                Language = "html",
                Links = new CodeSample.Link[]
                {
                    new CodeSample.Link(LinkType.Tooltip, "grid.HandlerForms", "HandlerForms")
                }
            },
            new CodeSample()
            {
                Title = "AllItems Query",
                ImportElementId = "AllItemsQuery",
                Url = "https://raw.githubusercontent.com/adamfoneil/Dapper.CX/code-samples/SampleApp.RazorPages/Queries/AllItems.cs"
            }
        };
    }
}
