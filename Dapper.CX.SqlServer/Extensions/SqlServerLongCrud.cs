using AO.Models.Interfaces;
using Dapper.CX.Classes;
using System.Data;
using System.Threading.Tasks;

namespace Dapper.CX.SqlServer.Extensions.Long
{
    public static partial class SqlServerLongCrud
    {
        public static async Task<TModel> GetAsync<TModel>(this IDbConnection connection, long id, IDbTransaction txn = null, IUserBase user = null)
        {
            var provider = new SqlServerLongCrudProvider();
            return await provider.GetAsync<TModel>(connection, id, txn, user);
        }

        public static async Task<TModel> GetWhereAsync<TModel>(this IDbConnection connection, object criteria, IDbTransaction txn = null, IUserBase user = null)
        {
            var provider = new SqlServerLongCrudProvider();
            return await provider.GetWhereAsync<TModel>(connection, criteria, txn, user);
        }

        public static async Task<long> InsertAsync<TModel>(this IDbConnection connection, TModel model, bool getIdentity = true, IDbTransaction txn = null, IUserBase user = null)
        {
            var provider = new SqlServerLongCrudProvider();
            return await provider.InsertAsync(connection, model, getIdentity, txn, user);
        }

        public static async Task UpdateAsync<TModel>(this IDbConnection connection, TModel model, ChangeTracker<TModel> changeTracker = null, IDbTransaction txn = null, IUserBase user = null)
        {
            var provider = new SqlServerLongCrudProvider();
            await provider.UpdateAsync(connection, model, changeTracker, txn);
        }

        public static async Task DeleteAsync<TModel>(this IDbConnection connection, long id, IDbTransaction txn = null, IUserBase user = null)
        {
            var provider = new SqlServerLongCrudProvider();
            await provider.DeleteAsync<TModel>(connection, id, txn, user);
        }

        public static async Task<long> SaveAsync<TModel>(this IDbConnection connection, TModel model, ChangeTracker<TModel> changeTracker = null, IDbTransaction txn = null, IUserBase user = null)
        {
            var provider = new SqlServerLongCrudProvider();
            return await provider.SaveAsync(connection, model, changeTracker, txn, user);
        }

        public static async Task<long> MergeAsync<TModel>(this IDbConnection connection, TModel model, ChangeTracker<TModel> changeTracker = null, IDbTransaction txn = null, IUserBase user = null)
        {
            var provider = new SqlServerIntCrudProvider();
            return await provider.MergeAsync(connection, model, changeTracker, txn, user);
        }

        public static async Task<bool> ExistsAsync<TModel>(this IDbConnection connection, long id)
        {
            var provider = new SqlServerLongCrudProvider();
            return await provider.ExistsAsync<TModel>(connection, id);
        }

        public static async Task<bool> ExistsWhereAsync<TModel>(this IDbConnection connection, object criteria, IDbTransaction txn = null)
        {
            var provider = new SqlServerLongCrudProvider();
            return await provider.ExistsWhereAsync<TModel>(connection, criteria, txn);
        }
    }
}
