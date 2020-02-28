using System;
using System.Data;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Dapper.CX.SqlServer.Extensions
{
    public static partial class SqlServerLongCrud
    {
        public static async Task<long> SaveAsync<TModel>(IDbConnection connection, TModel @object, params string[] columnNames)
        {
            var provider = new SqlServerIntCrudProvider();
            return await provider.SaveAsync(connection, @object, columnNames);
        }

        public static async Task UpdateAsync<TModel>(IDbConnection connection, TModel @object, params Expression<Func<TModel, object>>[] setColumns)
        {
            var provider = new SqlServerIntCrudProvider();
            await provider.UpdateAsync(connection, @object, setColumns);
        }
    }
}
