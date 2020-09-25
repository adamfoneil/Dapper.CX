using Dapper.CX.SqlServer.Services;
using SampleApp.Models;
using SampleApp.RazorPages.Interfaces;
using System.Collections.Generic;

namespace SampleApp.RazorPages.Pages
{
    public class StartupModel : BasePageModel, ICodeSample
    {
        public StartupModel(DapperCX<int, UserProfile> data) : base(data)
        {
        }     

        public void OnGet()
        {
        }

        public IEnumerable<CodeSample> Samples => new CodeSample[]
        {
            new CodeSample()
            {
                Title = "Claims Converter",
                Url = "https://raw.githubusercontent.com/adamfoneil/Dapper.CX/master/SampleApp.RazorPages/Services/UserProfileClaimsConverter.cs",
                ImportElementId = "claims-converter"
            },
            new CodeSample()
            {
                Title = "Claims Factory",
                Url = "https://raw.githubusercontent.com/adamfoneil/Dapper.CX/master/SampleApp.RazorPages/Services/UserProfileClaimsFactory.cs"
            },
            new CodeSample()
            {
                Title = "Startup",
                Url = "https://raw.githubusercontent.com/adamfoneil/Dapper.CX/master/SampleApp.RazorPages/Startup.cs"
            }
        };
    }
}
