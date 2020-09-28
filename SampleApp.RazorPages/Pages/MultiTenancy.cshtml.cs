using Dapper.CX.SqlServer.AspNetCore.Extensions;
using Dapper.CX.SqlServer.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SampleApp.Models;
using SampleApp.RazorPages.Queries.SelectLists;
using System.Threading.Tasks;

namespace SampleApp.RazorPages.Pages
{
    [Authorize]
    public partial class MultiTenancyModel : BasePageModel
    {
        public MultiTenancyModel(DapperCX<int, UserProfile> data) : base(data)
        {
        }

        public SelectList WorkspaceSelect { get; set; }
        public Workspace Workspace { get; set; }        

        public async Task OnGetAsync()
        {
            if (Data.HasUser)
            {
                WorkspaceSelect = await Data.QuerySelectListAsync(new WorkspaceSelect(), Data.User.WorkspaceId);
                Workspace = await Data.GetAsync<Workspace>(Data.User.WorkspaceId ?? 0);
            }
        }

        public async Task<RedirectResult> OnPostSetWorkspaceAsync(int workspaceId = 0)
        {
            Data.User.WorkspaceId = (workspaceId != 0) ? workspaceId : default(int?);
            var result = await Data.TryUpdateUserAsync(onException: SaveErrorMessage);
            return Redirect("/MultiTenancy");
        }

        public async Task<RedirectResult> OnPostSaveWorkspaceAsync(Workspace workspace)
        {
            await Data.TrySaveAsync(workspace,
                onSuccess: (id) => SaveSuccessMessage("Saved workspace successfully"),
                onException: SaveErrorMessage);

            return Redirect("/MultiTenancy");
        }
    }
}
