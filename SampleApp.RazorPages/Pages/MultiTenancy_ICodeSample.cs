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
                Title = "Implement ITenantUser<T>",
                Url = "https://raw.githubusercontent.com/adamfoneil/Dapper.CX/master/SampleApp.Services/Models/UserProfile_ext.cs",
                ImportElementId = "ITenantUserImpl",
                Links = new CodeSample.Link[]
                {
                    new CodeSample.Link(LinkType.Tooltip, "TenantId", "TenantIdTooltip"),
                    new CodeSample.Link(LinkType.Tooltip, "ITenantUser&lt;int&gt;", "TenantIdInterface")
                }
            },
            new CodeSample()
            {
                Title = "Implement ITenantIsolated<T> on Item model",
                Url = "https://raw.githubusercontent.com/adamfoneil/Dapper.CX/master/SampleApp.Services/Models/Item_TenantIsolated.cs",
                ImportElementId = "ITenantIsolatedImpl",
                Links = new CodeSample.Link[]
                {
                    new CodeSample.Link(LinkType.Tooltip, "GetTenantIdAsync", "GetTenantIdTooltip")
                }
            },
            new CodeSample()
            {
                Title = "Implement ITenantIsolated<T> on ItemPrice model",
                Url = "https://raw.githubusercontent.com/adamfoneil/Dapper.CX/master/SampleApp.Services/Models/ItemPrice_TenantIsolated.cs",
                ImportElementId = "ITenantIsolatedItemPrice",
                Links = new CodeSample.Link[]
                {
                    new CodeSample.Link(LinkType.Tooltip, "GetTenantIdAsync", "GetTenantIdQuery")
                }
            }
        };
    }
}
