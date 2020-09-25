using Dapper.CX.SqlServer.AspNetCore.Extensions;
using Dapper.CX.SqlServer.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SampleApp.Models;
using SampleApp.RazorPages.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SampleApp.RazorPages.Pages
{
    [Authorize]
    public class ItemsModel : BasePageModel, ICodeSample
    {
        public ItemsModel(DapperCX<int, UserProfile> data) : base(data)
        {
        }

        [BindProperty(SupportsGet = true)]
        public int Id { get; set; }
        
        public SelectList ItemSelect { get; set; }

        public Item Item { get; set; }

        public async Task OnGetAsync()
        {
            Item = await Data.GetAsync<Item>(Id);

            ItemSelect = await Data.QuerySelectListAsync(new Queries.SelectLists.ItemSelect()
            {
                WorkspaceId = Data.User.WorkspaceId ?? 0
            }, Id);
        }

        public async Task<RedirectResult> OnPostSaveItemAsync(Item item) => 
            await Data.SaveAndRedirectAsync(item, (model, exc) => (model.Id != 0) ? Redirect($"/Items/{model.Id}") : Redirect("/Items"), 
                beforeSave: (model) => model.WorkspaceId = Data.User.WorkspaceId ?? 0,
                onSuccess: (id) => SaveSuccessMessage($"Item {id} updated successfully."), 
                onException: (model, exc) => SaveErrorMessage(exc));

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
                Language = "js"
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
