using Dapper.CX.SqlServer.Abstract;
using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Dapper.CX.SqlServer
{
    public class SqlServerIntCmd : SqlServerCmd<int>
    {
        public SqlServerIntCmd(string tableName, string identityColumn)
        {
            TableName = tableName;
            IdentityColumn = identityColumn;
        }

        public SqlServerIntCmd(object @object)
        {
            InitializeFromObject(@object);
        }

        public SqlServerIntCmd(Type type)
        {
            InitializeFromType(type);
        }

        public static async Task<SqlServerIntCmd> FromSchemaAsync(IDbConnection connection, string schema, string tableName)
        {
            string identityCol = await GetIdentityColumnFromSchema(connection, schema, tableName);

            var result = new SqlServerIntCmd($"{schema}.{tableName}", identityCol);
            
            var keyColumns = await GetKeyColumnsFromSchema(connection, schema, tableName);
            var allColumns = await GetColumnsFromSchema(connection, schema, tableName, keyColumns);

            foreach (var col in keyColumns) result.Add("#" + col, null);
            foreach (var col in allColumns.Except(keyColumns)) result.Add(col, null);

            return result;
        }

        protected override int ConvertIdentity(object identity)
        {
            return Convert.ToInt32(identity);
        }
    }
}
