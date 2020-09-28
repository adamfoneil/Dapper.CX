using SampleApp.RazorPages.Interfaces;
using System.Collections.Generic;

namespace SampleApp.RazorPages.Pages
{
    public partial class MultiTenancyModel : ICodeSample
    {
        public IEnumerable<CodeSample> Samples => new CodeSample[]
        {
            new CodeSample()
            {
                Title = "UserProfile",
                Url = "https://raw.githubusercontent.com/adamfoneil/Dapper.CX/code-samples/SampleApp.Services/Models/UserProfile_ext.cs",
                Links = new CodeSample.Link[]
                {
                    new CodeSample.Link(LinkType.Tooltip, "TenantId", "TenantIdTooltip"),
                    new CodeSample.Link(LinkType.Tooltip, "ITenantUser&lt;int&gt;", "TenantIdInterface")
                }
            },
            new CodeSample()
            {
                Title = "Item.cs",
                Url = "https://raw.githubusercontent.com/adamfoneil/Dapper.CX/code-samples/SampleApp.Services/Models/Item_ext.cs",
                Links = new CodeSample.Link[]
                {
                    new CodeSample.Link(LinkType.Tooltip, "GetTenantIdAsync", "GetTenantIdTooltip")
                }
            },
            new CodeSample()
            {
                Title = "ITenantUser<T>",
                Url = "https://raw.githubusercontent.com/adamfoneil/Models/master/Models/Interfaces/ITenantUser.cs",
                ImportElementId = "ITenantUser"
            },
            new CodeSample()
            {
                Title = "ITenantIsolated<T>",
                Url = "https://raw.githubusercontent.com/adamfoneil/Models/master/Models/Interfaces/ITenantIsolated.cs"
            }
        };
    }
}
