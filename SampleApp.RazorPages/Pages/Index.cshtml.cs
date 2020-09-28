using Dapper.CX.SqlServer.Services;
using SampleApp.Models;

namespace SampleApp.RazorPages.Pages
{
    public class IndexModel : BasePageModel
    {
        public IndexModel(DapperCX<int, UserProfile> data) : base(data)
        {
        }        
    }
}
