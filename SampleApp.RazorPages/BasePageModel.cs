using Dapper.CX.SqlServer.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SampleApp.Models;
using SampleApp.RazorPages.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SampleApp.RazorPages
{
    public class BasePageModel : PageModel
    {
        public BasePageModel(DataAccess data)
        {
            Data = data;
        }

        public DataAccess Data { get; }

        protected async Task SaveSuccessMessage(string message)
        {
            TempData.Remove("success");
            TempData.Add("success", message);
            await Task.CompletedTask;
        }

        protected async Task SaveErrorMessage(Exception exc)
        {
            TempData.Remove("error");
            TempData.TryAdd("error", exc.Message);
            await Task.CompletedTask;
        }
    }
}
