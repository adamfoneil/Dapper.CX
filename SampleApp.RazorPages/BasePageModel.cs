using Dapper.CX.SqlServer.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SampleApp.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SampleApp.RazorPages
{
    public class BasePageModel : PageModel
    {
        public BasePageModel(DapperCX<int, UserProfile> data)
        {
            Data = data;
        }

        public DapperCX<int, UserProfile> Data { get; }

        protected async Task SaveSuccessMessage(string message)
        {
            TempData.Remove("success");
            TempData.Add("success", message);
            await Task.CompletedTask;
        }

        protected void SaveErrorMessage(Exception exc)
        {
            TempData.Remove("error");
            TempData.TryAdd("error", exc.Message);
        }
    }
}
