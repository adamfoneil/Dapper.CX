using System;
using System.Data;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Dapper.CX.SqlServer.Extensions.Long
{
    public static partial class SqlServerLongCrud
    {
        public static async Task<long> SaveAsync<TModel>(this IDbConnection connection, TModel @object, params string[] columnNames)
        {
            var provider = new SqlServerLongCrudProvider();
            return await provider.SaveAsync(connection, @object, columnNames);
        }

        public static async Task UpdateAsync<TModel>(this IDbConnection connection, TModel @object, params Expression<Func<TModel, object>>[] setColumns)
        {
            var provider = new SqlServerLongCrudProvider();
            await provider.UpdateAsync(connection, @object, setColumns);
        }
    }
}
