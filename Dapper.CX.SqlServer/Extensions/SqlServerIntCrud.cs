using System;
using System.Data;
using System.Threading.Tasks;

namespace Dapper.CX.SqlServer.Extensions
{
    public static class SqlServerIntCrud
    {
        public static async Task<TModel> FindAsync<TModel>(this IDbConnection connection, int id)
        {
            var cmd = new SqlServerIntCmd(typeof(TModel));
            return await cmd.GetAsync<TModel>(connection, id);
        }

        public static async Task<int> InsertAsync<TModel>(this IDbConnection connection, TModel model)
        {
            var cmd = new SqlServerIntCmd(model);
            return await cmd.InsertAsync(connection);
        }

        public static async Task UpdateAsync<TModel>(this IDbConnection connection, TModel model)
        {
            var cmd = new SqlServerIntCmd(model);
            await cmd.UpdateAsync(connection);
        }

        public static async Task DeleteAsync<TModel>(this IDbConnection connection, int id)
        {
            var cmd = new SqlServerIntCmd(typeof(TModel));
            await cmd.DeleteAsync(connection, id);
        }

        public static async Task<int> SaveAsync<TModel>(this IDbConnection connection, TModel model)
        {
            var cmd = new SqlServerIntCmd(model);
            return await cmd.SaveAsync(connection);
        }

        public static async Task<int> MergeAsync<TModel>(this IDbConnection connection, TModel model)
        {
            var cmd = new SqlServerIntCmd(model);
            return await cmd.MergeAsync(connection);
        }
    }
}
