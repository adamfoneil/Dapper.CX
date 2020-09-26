using SampleApp.RazorPages.Interfaces;
using System.Collections.Generic;

namespace SampleApp.RazorPages.Pages
{
    public partial class ItemsModel : ICodeSample
    {
        public IEnumerable<CodeSample> Samples => new CodeSample[]
        {
            new CodeSample()
            {
                Title = "Code Behind",
                Url = "https://raw.githubusercontent.com/adamfoneil/Dapper.CX/master/SampleApp.RazorPages/Pages/Items.cshtml.cs"
            },
            new CodeSample()
            {
                Title = "Razor",
                Url = "https://raw.githubusercontent.com/adamfoneil/Dapper.CX/master/SampleApp.RazorPages/Pages/Items.cshtml",
                Language = "html"
            },
            new CodeSample()
            {
                Title = "JavaScript",
                Url = "https://raw.githubusercontent.com/adamfoneil/Dapper.CX/master/SampleApp.RazorPages/wwwroot/js/Items.js",
                Language = "js",
                ImportElementId = "goto-item"
            },
            new CodeSample()
            {
                Title = "Model Class",
                Url = "https://raw.githubusercontent.com/adamfoneil/Dapper.CX/master/SampleApp.Models/Item.cs"
            },
            new CodeSample()
            {
                Title = "Select List Query",
                Url = "https://raw.githubusercontent.com/adamfoneil/Dapper.CX/code-samples/SampleApp.RazorPages/Queries/SelectLists/ItemSelect.cs"
            }
        };
    }
}
