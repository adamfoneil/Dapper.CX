using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;

namespace Dapper.TinyCrud.SqlServer.Extensions
{
    public static class SqlServerCrud
    {
        public static async Task<int> SaveAsync<TModel>(this IDbConnection connection, TModel model)
        {
            var cmd = new SqlServerCmd(model);
            return await cmd.SaveAsync(connection, )
        }
    }
}
