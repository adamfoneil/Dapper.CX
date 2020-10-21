using System.Data;
using System.Threading.Tasks;

namespace Dapper.CX.Extensions
{
    public static class ConnectionExtensions
    {
        public static async Task<bool> RowExistsAsync(this IDbConnection connection, string fromWhere, object parameters = null, IDbTransaction transaction = null)
        {
            return ((await connection.QueryFirstOrDefaultAsync<int?>($"SELECT 1 FROM {fromWhere}", parameters, transaction) ?? 0) == 1);
        }

        public static async Task<bool> TableExistsAsync(this IDbConnection connection, string schema, string tableName, IDbTransaction transaction = null)
        {
            return await RowExistsAsync(connection, "[sys].[tables] WHERE SCHEMA_NAME([schema_id])=@schema AND [name]=@tableName", new { schema, tableName }, transaction);
        }

        public static async Task<bool> SchemaExistsAsync(this IDbConnection connection, string schema, IDbTransaction transaction = null)
        {
            return await RowExistsAsync(connection, "[sys].[schemas] WHERE [name]=@schema", new { schema }, transaction);
        }
    }
}
