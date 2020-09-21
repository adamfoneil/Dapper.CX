using AO.Models.Interfaces;
using Dapper.CX.Classes;
using System;
using System.Data;
using System.Threading.Tasks;

namespace Dapper.CX.SqlServer.Extensions.Long
{
    public static partial class SqlServerLongCrud
    {
        private static SqlServerCrudProvider<long> GetProvider() => new SqlServerCrudProvider<long>(id => Convert.ToInt64(id));

        public static async Task<TModel> GetAsync<TModel>(this IDbConnection connection, long id, IDbTransaction txn = null, IUserBase user = null)
        {
            return await GetProvider().GetAsync<TModel>(connection, id, txn, user);
        }

        public static async Task<TModel> GetWhereAsync<TModel>(this IDbConnection connection, object criteria, IDbTransaction txn = null, IUserBase user = null)
        {
            return await GetProvider().GetWhereAsync<TModel>(connection, criteria, txn, user);
        }

        public static async Task<long> InsertAsync<TModel>(this IDbConnection connection, TModel model, bool getIdentity = true, IDbTransaction txn = null, IUserBase user = null)
        {
            return await GetProvider().InsertAsync(connection, model, getIdentity, txn, user);
        }

        public static async Task UpdateAsync<TModel>(this IDbConnection connection, TModel model, ChangeTracker<TModel> changeTracker = null, IDbTransaction txn = null, IUserBase user = null)
        {
            await GetProvider().UpdateAsync(connection, model, changeTracker, txn);
        }

        public static async Task DeleteAsync<TModel>(this IDbConnection connection, long id, IDbTransaction txn = null, IUserBase user = null)
        {
            await GetProvider().DeleteAsync<TModel>(connection, id, txn, user);
        }

        public static async Task<long> SaveAsync<TModel>(this IDbConnection connection, TModel model, ChangeTracker<TModel> changeTracker = null, IDbTransaction txn = null, IUserBase user = null)
        {
            return await GetProvider().SaveAsync(connection, model, changeTracker, txn, user);
        }

        public static async Task<long> MergeAsync<TModel>(this IDbConnection connection, TModel model, ChangeTracker<TModel> changeTracker = null, IDbTransaction txn = null, IUserBase user = null)
        {
            return await GetProvider().MergeAsync(connection, model, changeTracker, txn, user);
        }

        public static async Task<bool> ExistsAsync<TModel>(this IDbConnection connection, long id)
        {
            return await GetProvider().ExistsAsync<TModel>(connection, id);
        }

        public static async Task<bool> ExistsWhereAsync<TModel>(this IDbConnection connection, object criteria, IDbTransaction txn = null)
        {
            return await GetProvider().ExistsWhereAsync<TModel>(connection, criteria, txn);
        }
    }
}
