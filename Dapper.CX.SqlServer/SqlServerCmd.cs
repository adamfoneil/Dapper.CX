using Dapper.CX.Abstract;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Dapper.CX.SqlServer
{
    public class SqlServerCmd : SqlCmdDictionary
    {
        public SqlServerCmd()
        {
        }

        public SqlServerCmd(string tableName, string identityColumn, IEnumerable<string> columnNames = null) : base(tableName, identityColumn, columnNames)
        {
        }

        public static async Task<SqlServerCmd> FromTableSchemaAsync(IDbConnection connection, string schemaName, string tableName, IEnumerable<string> keyColumns)
        {
            string identityCol = await connection.QuerySingleOrDefaultAsync<string>(
                @"SELECT [col].[name]
                FROM [sys].[columns] [col]
                INNER JOIN [sys].[tables] [t] ON [col].[object_id]=[t].[object_id]
                WHERE 
	                SCHEMA_NAME([t].[schema_id])=@schemaName AND
	                [t].[name]=@tableName AND	                
                    [col].[is_identity]=1", new { schemaName, tableName });

            SqlServerCmd result = new SqlServerCmd($"{schemaName}.{tableName}", identityCol);

            var columns = await connection.QueryAsync<string>(
                @"SELECT [col].[name]
                FROM [sys].[columns] [col]
                INNER JOIN [sys].[tables] [t] ON [col].[object_id]=[t].[object_id]
                WHERE 
	                SCHEMA_NAME([t].[schema_id])=@schemaName AND
	                [t].[name]=@tableName AND
	                [col].[is_computed]=0 AND
                    [col].[is_identity]=0", new { schemaName, tableName });

            foreach (var col in keyColumns) result.Add(KeyColumnPrefix + col, null);
            foreach (var col in columns.Except(keyColumns)) result.Add(col, null);
            return result;
        }

        public static async Task<SqlServerCmd> FromTableSchemaAsync(IDbConnection connection, string schemaName, string tableName)
        {
            var keyColumns = await connection.QueryAsync<string>(
                @"SELECT [col].[name]
                FROM [sys].[indexes] [ndx]
                INNER JOIN [sys].[tables] [t] ON [ndx].[object_id]=[t].[object_id]
                INNER JOIN [sys].[index_columns] [xcol] ON 
	                [ndx].[object_id]=[xcol].[object_id] AND
	                [ndx].[index_id]=[xcol].[index_id]
                INNER JOIN [sys].[columns] [col] ON
	                [t].[object_id]=[col].[object_id] AND
	                [xcol].[column_id]=[col].[column_id]
                WHERE
	                SCHEMA_NAME([t].[schema_id])=@schemaName AND
	                [t].[name]=@tableName AND
	                [ndx].[is_primary_key]=1 AND
                    [col].[is_identity]=0", new { schemaName, tableName });

            return await FromTableSchemaAsync(connection, schemaName, tableName, keyColumns);
        }

        public static async Task<SqlServerCmd> FromQueryAsync(IDbConnection connection, string sql, object parameters = null, string omitIdentityColumn = null)
        {
            // help from https://stackoverflow.com/a/26661203/2023653
            var row = await connection.QuerySingleOrDefaultAsync(sql, parameters);
            var dictionary = row as IDictionary<string, object>;
            var result = new SqlServerCmd();
            foreach (var kp in dictionary)
            {
                if (!kp.Key.Equals(omitIdentityColumn))
                {
                    result.Add(kp.Key, kp.Value);
                }                
            }
            return result;            
        }

        protected override string SelectIdentityCommand => "SELECT SCOPE_IDENTITY();";

        protected override char StartDelimiter => '[';

        protected override char EndDelimiter => ']';

        public override IDbCommand GetInsertCommand(IDbConnection connection)
        {
            var cmd = new SqlCommand(GetInsertStatement(), connection as SqlConnection);
            AddParameters(cmd);
            return cmd;
        }

        public override IDbCommand GetUpdateCommand(IDbConnection connection)
        {
            var cmd = new SqlCommand(GetUpdateStatement(), connection as SqlConnection);
            AddParameters(cmd);
            cmd.Parameters.AddWithValue(IdentityColumn, DBNull.Value);
            return cmd;
        }

        private void AddParameters(SqlCommand cmd)
        {
            foreach (var keyPair in this)
            {
                cmd.Parameters.AddWithValue(ParseColumnName(keyPair.Key), keyPair.Value ?? DBNull.Value);
            }
        }
    }
}
