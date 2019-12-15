using Dapper.CX.Classes;
using Dapper.CX.Enums;
using System;
using System.Data;
using System.Threading.Tasks;

namespace Dapper.CX.SqlServer.Extensions
{
    public static class SqlServerIntCrud
    {
        public static async Task<TModel> GetAsync<TModel>(this IDbConnection connection, int id)
        {
            var provider = new SqlServerIntCrudProvider();
            return await provider.GetAsync<TModel>(connection, id);
        }

        public static async Task<TModel> GetWhereAsync<TModel>(this IDbConnection connection, object criteria)
        {
            var provider = new SqlServerIntCrudProvider();
            return await provider.GetWhereAsync<TModel>(connection, criteria);
        }

        public static async Task<int> InsertAsync<TModel>(this IDbConnection connection, TModel model, Action<TModel, SaveAction> onSave = null)
        {
            var provider = new SqlServerIntCrudProvider();
            return await provider.InsertAsync(connection, model, onSave);
        }

        public static async Task UpdateAsync<TModel>(this IDbConnection connection, TModel model, ChangeTracker<TModel> changeTracker = null, Action<TModel, SaveAction> onSave = null)
        {
            var provider = new SqlServerIntCrudProvider();
            await provider.UpdateAsync(connection, model, changeTracker, onSave);
        }

        public static async Task DeleteAsync<TModel>(this IDbConnection connection, int id)
        {
            var provider = new SqlServerIntCrudProvider();
            await provider.DeleteAsync<TModel>(connection, id);
        }

        public static async Task<int> SaveAsync<TModel>(this IDbConnection connection, TModel model, ChangeTracker<TModel> changeTracker = null, Action<TModel, SaveAction> onSave = null)
        {
            var provider = new SqlServerIntCrudProvider();
            return await provider.SaveAsync(connection, model, changeTracker, onSave);
        }

        public static async Task<int> MergeAsync<TModel>(this IDbConnection connection, TModel model, ChangeTracker<TModel> changeTracker = null, Action<TModel, SaveAction> onSave = null)
        {
            var provider = new SqlServerIntCrudProvider();
            return await provider.MergeAsync(connection, model, changeTracker, onSave);
        }
    }
}
