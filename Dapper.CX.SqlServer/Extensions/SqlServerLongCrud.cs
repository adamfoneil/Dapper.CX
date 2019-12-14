using Dapper.CX.Classes;
using System.Data;
using System.Threading.Tasks;

namespace Dapper.CX.SqlServer.Extensions
{
    public static class SqlServerLongCrud
    {
        public static async Task<TModel> GetAsync<TModel>(this IDbConnection connection, int id)
        {
            var provider = new SqlServerLongCrudProvider();
            return await provider.GetAsync<TModel>(connection, id);
        }

        public static async Task<TModel> GetWhereAsync<TModel>(this IDbConnection connection, object criteria)
        {
            var provider = new SqlServerIntCrudProvider();
            return await provider.GetWhereAsync<TModel>(connection, criteria);
        }

        public static async Task<long> InsertAsync<TModel>(this IDbConnection connection, TModel model)
        {
            var provider = new SqlServerLongCrudProvider();
            return await provider.InsertAsync(connection, model);
        }

        public static async Task UpdateAsync<TModel>(this IDbConnection connection, TModel model, ChangeTracker<TModel> changeTracker = null)
        {
            var provider = new SqlServerLongCrudProvider();
            await provider.UpdateAsync(connection, model, changeTracker);
        }

        public static async Task DeleteAsync<TModel>(this IDbConnection connection, int id)
        {
            var provider = new SqlServerLongCrudProvider();
            await provider.DeleteAsync<TModel>(connection, id);
        }

        public static async Task<long> SaveAsync<TModel>(this IDbConnection connection, TModel model, ChangeTracker<TModel> changeTracker = null)
        {
            var provider = new SqlServerLongCrudProvider();
            return await provider.SaveAsync(connection, model, changeTracker);
        }
    }
}
