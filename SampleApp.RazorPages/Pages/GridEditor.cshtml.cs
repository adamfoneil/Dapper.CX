using Dapper.CX.SqlServer.AspNetCore.Extensions;
using Dapper.CX.SqlServer.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SampleApp.Models;
using SampleApp.RazorPages.Queries;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SampleApp.RazorPages.Pages
{
    [Authorize]
    public partial class GridEditorModel : BasePageModel
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
    }
}
