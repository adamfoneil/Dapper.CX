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
        public static async Task<RedirectResult> SaveAndRedirectAsync<TModel, TIdentity, TUser>(
            this SqlServerCrudService<TIdentity, TUser> crudService, TModel model, Func<TModel, Exception, RedirectResult> redirect, 
            ChangeTracker<TModel> changeTracker = null, Action<TModel> beforeSave = null,
            Func<TModel, Task> onSuccess = null, Func<TModel, Exception, Task> onException = null) where TUser : IUserBase
        {
            beforeSave?.Invoke(model);

            try
            {
                var result = await crudService.SaveAsync(model, changeTracker);
                onSuccess?.Invoke(model);
                return redirect.Invoke(model, null);
            }
            catch (Exception exc)
            {
                onException?.Invoke(model, exc);
                return redirect.Invoke(model, exc);
            }
        }
    }
}
