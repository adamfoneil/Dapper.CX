using AO.Models.Interfaces;
using Dapper.CX.Classes;
using Dapper.CX.SqlServer.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Dapper.CX.SqlServer.AspNetCore.Extensions
{
    public static class SqlCrudServiceExtensions
    {
        public static async Task<IActionResult> SaveAndRedirectAsync<TModel, TIdentity, TUser>(
            this SqlServerCrudService<TIdentity, TUser> crudService, TModel model, Func<TIdentity, IActionResult> redirect, 
            ChangeTracker<TModel> changeTracker = null, Action<TModel> beforeSave = null,
            Func<TIdentity, Task> onSuccess = null, Func<Exception, Task> onException = null) where TUser : IUserBase
        {
            beforeSave?.Invoke(model);
            var result = await crudService.TrySaveAsync(model, changeTracker, onSuccess, onException);
            return redirect.Invoke(result);
        }
    }
}
