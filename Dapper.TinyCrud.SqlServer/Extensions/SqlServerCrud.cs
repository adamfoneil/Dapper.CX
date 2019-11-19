using System.Data;
using System.Threading.Tasks;

namespace Dapper.TinyCrud.SqlServer.Extensions
{
    public static class SqlServerIntIdentity
    {
        public static async Task<int> SaveAsync<TModel>(this IDbConnection connection, TModel model)
        {
            var cmd = new SqlServerIntCmd(model);
            return await cmd.SaveAsync(connection);
        }
    }
}
