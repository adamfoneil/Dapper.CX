using Dapper.CX.SqlServer.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SampleApp.Models;
using SampleApp.RazorPages.Queries;
using SampleApp.RazorPages.Queries.SelectLists;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SampleApp.RazorPages.Pages
{
    public class IndexModel : BasePageModel
    {
        public IndexModel(SqlServerCrudService<int, UserProfile> crud) : base(crud)
        {
        }
        

        public SelectList WorkspaceSelect { get; set; }
        public IEnumerable<Item> AllItems { get; set; }
        public Workspace Workspace { get; set; }

        public async Task OnGetAsync()
        {
            if (Data.HasCurrentUser)
            {
                WorkspaceSelect = await Data.QuerySelectListAsync(new WorkspaceSelect(), Data.CurrentUser.WorkspaceId);
                AllItems = await Data.QueryAsync(new AllItems() { WorkspaceId = Data.CurrentUser.WorkspaceId ?? 0, IsActive = true });
                Workspace = await Data.GetAsync<Workspace>(Data.CurrentUser.WorkspaceId ?? 0);
            }
        }

        public async Task<RedirectResult> OnPostSetWorkspaceAsync(int workspaceId = 0)
        {
            Data.CurrentUser.WorkspaceId = (workspaceId != 0) ? workspaceId : default(int?);
            var result = await Data.TryUpdateUserAsync(onException: SaveErrorMessage);
            return Redirect("/Index");
        }

        public async Task<RedirectResult> OnPostSaveWorkspaceAsync(Workspace workspace)
        {
            await Data.TrySaveAsync(workspace, 
                onSuccess: (id) => SaveSuccessMessage("Saved workspace successfully"),
                onException: SaveErrorMessage);

            return Redirect("/");
        }
    }
}
