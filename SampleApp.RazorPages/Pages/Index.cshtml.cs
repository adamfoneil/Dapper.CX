using Dapper.CX.SqlServer.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using SampleApp.Models;
using SampleApp.RazorPages.Queries;
using SampleApp.RazorPages.Queries.SelectLists;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SampleApp.RazorPages.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(ILogger<IndexModel> logger, SqlServerCrudService<int, UserProfile> crud)
        {
            _logger = logger;
            Data = crud;
        }

        public SqlServerCrudService<int, UserProfile> Data { get; }
        public SelectList WorkspaceSelect { get; set; }
        public IEnumerable<Item> AllItems { get; set; }

        public async Task OnGetAsync()
        {
            if (Data.HasCurrentUser)
            {
                WorkspaceSelect = await Data.QuerySelectListAsync(new WorkspaceSelect(), Data.CurrentUser.WorkspaceId);
                AllItems = await Data.QueryAsync(new AllItems() { WorkspaceId = Data.CurrentUser.WorkspaceId ?? 0, IsActive = true });
            }
        }

        public async Task<RedirectResult> OnPostSetWorkspaceAsync(int workspaceId = 0)
        {
            Data.CurrentUser.WorkspaceId = (workspaceId != 0) ? workspaceId : default(int?);
            var result = await Data.TryUpdateUserAsync(onException: async (exc) => TempData.Add("error", exc.Message));
            return Redirect("/Index");
        }
    }
}
