using AO.Models.Interfaces;
using Dapper.CX.Classes;
using Dapper.CX.SqlServer.Services;
using Dapper.QX;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
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

        public static async Task<IEnumerable<TResult>> QueryAsync<TResult, TIdentity, TUser>(
            this SqlServerCrudService<TIdentity, TUser> crudService,
            Query<TResult> query) where TUser : IUserBase
        {
            using (var cn = crudService.GetConnection())
            {
                return await query.ExecuteAsync(cn);
            }
        }

        public static async Task<TResult> QuerySingleAsync<TResult, TIdentity, TUser>(
            this SqlServerCrudService<TIdentity, TUser> crudService,
            Query<TResult> query) where TUser : IUserBase
        {
            using (var cn = crudService.GetConnection())
            {
                return await query.ExecuteSingleAsync(cn);
            }
        }

        public static async Task<TResult> QuerySingleOrDefaultAsync<TResult, TIdentity, TUser>(
            this SqlServerCrudService<TIdentity, TUser> crudService,
            Query<TResult> query) where TUser : IUserBase
        {
            using (var cn = crudService.GetConnection())
            {
                return await query.ExecuteSingleOrDefaultAsync(cn);
            }
        }

        public static async Task<SelectList> QuerySelectListAsync<TIdentity, TUser>(
            this SqlServerCrudService<TIdentity, TUser> crudService,
            Query<SelectListItem> query, object selectedValue = null) where TUser : IUserBase
        {
            using (var cn = crudService.GetConnection())
            {
                var items = await query.ExecuteAsync(cn);
                return new SelectList(items, "Value", "Text", selectedValue);
            }
        }
    }
}
