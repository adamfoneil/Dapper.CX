using Dapper.CX.SqlServer.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SampleApp.Models;

namespace SampleApp.RazorPages
{
    public class BasePageModel : PageModel
    {
        public BasePageModel(SqlServerCrudService<int, UserProfile> crud)
        {
            Data = crud;
        }

        public SqlServerCrudService<int, UserProfile> Data { get; }
    }
}
