using Dapper.CX.SqlServer.AspNetCore.Extensions;
using Dapper.CX.SqlServer.Services;
using Microsoft.AspNetCore.Mvc;
using SampleApp.Models;
using SampleApp.RazorPages.Interfaces;
using SampleApp.RazorPages.Queries;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SampleApp.RazorPages.Pages
{
    public class GridEditorModel : BasePageModel, ICodeSample
    {
        public GridEditorModel(DapperCX<int, UserProfile> data) : base(data)
        {
        }

        public IEnumerable<Item> AllItems { get; set; }        

        public async Task OnGetAsync()
        {
            if (Data.HasUser)
            {
                AllItems = await Data.QueryAsync(new AllItems() 
                { 
                    WorkspaceId = Data.User.WorkspaceId ?? 0, 
                    IsActive = true 
                });
            }
        }

        public async Task<RedirectResult> OnPostSaveItemAsync(Item item)
        {
            await Data.TrySaveAsync(item, onException: SaveErrorMessage);
            return Redirect("/ItemGrid");
        }

        public async Task<RedirectResult> OnPostDeleteItemAsync(int id)
        {
            await Data.TryDeleteAsync<Item>(id, onException: SaveErrorMessage);
            return Redirect("/ItemGrid");
        }

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
