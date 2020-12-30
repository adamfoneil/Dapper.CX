using AO.Models.Interfaces;
using Dapper.CX.Classes;
using Dapper.CX.SqlServer.AspNetCore.Classes;
using Dapper.CX.SqlServer.Services;
using Dapper.QX;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dapper.CX.SqlServer.AspNetCore.Extensions
{
    public static class DapperCXExtensions
    {
        public static async Task<RedirectResult> SaveAndRedirectAsync<TModel, TIdentity, TUser>(
            this DapperCX<TIdentity, TUser> crudService, TModel model, Func<TModel, Exception, RedirectResult> redirect,
            ChangeTracker<TModel> changeTracker = null, Action<TModel> beforeSave = null,
            Func<TModel, Task> onSuccess = null, Action<TModel, Exception> onException = null) where TUser : IUserBase
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

        public static async Task<RedirectResult> DeleteAndRedirectAsync<TModel, TIdentity, TUser>(
            this DapperCX<TIdentity, TUser> crudService, TIdentity id, Func<TModel, Exception, RedirectResult> redirect,
            Func<TModel, Task> onSuccess = null, Action<TModel, Exception> onException = null) where TUser : IUserBase
        {
            var model = await crudService.GetAsync<TModel>(id);

            try
            {
                await crudService.DeleteAsync<TModel>(id);
                onSuccess?.Invoke(model);
                return redirect.Invoke(model, null);
            }
            catch (Exception exc)
            {
                onException?.Invoke(model, exc);
                return redirect.Invoke(model, exc);
            }
        }

        public static async Task<RedirectResult> DeleteAndRedirectAsync<TModel, TIdentity, TUser>(
            this DapperCX<TIdentity, TUser> crudService, TIdentity id, string redirect) where TUser : IUserBase
        {
            await crudService.DeleteAsync<TModel>(id);
            return new RedirectResult(redirect);
        }

        public static async Task<IEnumerable<TResult>> QueryAsync<TResult, TIdentity, TUser>(
            this DapperCX<TIdentity, TUser> crudService,
            Query<TResult> query) where TUser : IUserBase
        {
            using (var cn = crudService.GetConnection())
            {
                return await query.ExecuteAsync(cn);
            }
        }

        public static async Task<IEnumerable<TModel>> QueryAsync<TModel, TIdentity, TUser>(
            this DapperCX<TIdentity, TUser> crudService, object criteria) where TUser : IUserBase
        {
            using (var cn = crudService.GetConnection())
            {

            }
        }

        public static async Task<TResult> QuerySingleAsync<TResult, TIdentity, TUser>(
            this DapperCX<TIdentity, TUser> crudService,
            Query<TResult> query) where TUser : IUserBase
        {
            using (var cn = crudService.GetConnection())
            {
                return await query.ExecuteSingleAsync(cn);
            }
        }

        public static async Task<TResult> QuerySingleOrDefaultAsync<TResult, TIdentity, TUser>(
            this DapperCX<TIdentity, TUser> crudService,
            Query<TResult> query) where TUser : IUserBase
        {
            using (var cn = crudService.GetConnection())
            {
                return await query.ExecuteSingleOrDefaultAsync(cn);
            }
        }

        public static async Task<SelectList> QuerySelectListAsync<TIdentity, TUser>(
            this DapperCX<TIdentity, TUser> crudService,
            Query<SelectListItem> query, object selectedValue = null) where TUser : IUserBase
        {
            using (var cn = crudService.GetConnection())
            {
                return await SelectListQuery.ExecuteInternalAsync(query, cn, selectedValue);
            }
        }

        public static async Task<SelectList> QuerySelectListAsync<TIdentity, TUser>(
            this DapperCX<TIdentity, TUser> crudService,
            SelectListQuery query, object selectedValue = null) where TUser : IUserBase
        {
            using (var cn = crudService.GetConnection())
            {
                return await query.ExecuteSelectListAsync(cn, selectedValue);
            }
        }
    }
}
