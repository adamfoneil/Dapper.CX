using Dapper.CX.Abstract;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace Dapper.CX.SqlServer.Abstract
{
    public abstract class SqlServerCmd<TIdentity> : SqlCmdDictionary<TIdentity>
    {
        public SqlServerCmd()
        {
        }

        public SqlServerCmd(string tableName, string identityColumn)
        {
            TableName = tableName;
            IdentityColumn = identityColumn;
        }

        public SqlServerCmd(object @object)
        {
            InitializeFromObject(@object);
        }

        public SqlServerCmd(Type type)
        {
            InitializeFromType(type);
        }

        protected override string SelectIdentityCommand => "SELECT SCOPE_IDENTITY()";

        protected override char StartDelimiter => '[';

        protected override char EndDelimiter => ']';

        protected override Type[] SupportedTypes => new Type[] 
        {
            typeof(string), 
            typeof(int),
            typeof(DateTime),
            typeof(bool),
            typeof(long),
            typeof(decimal),
            typeof(double),
            typeof(float)
        };

        public async Task InitializeFromSchema(IDbConnection connection, string schema, string tableName)
        {
            IdentityColumn = await GetIdentityColumnFromSchema(connection, schema, tableName);

            var keyColumns = await GetKeyColumnsFromSchema(connection, schema, tableName);
            var allColumns = await GetColumnsFromSchema(connection, schema, tableName, keyColumns);

            foreach (var col in keyColumns) Add("#" + col, null);
            foreach (var col in allColumns.Except(keyColumns)) Add(col, null);
        }

        public override IDbCommand GetInsertCommand(IDbConnection connection)
        {
            var result = new SqlCommand(GetInsertStatement(), connection as SqlConnection);
            AddParameters(result);
            return result;
        }

        public override IDbCommand GetUpdateCommand(IDbConnection connection)
        {
            var result = new SqlCommand(GetUpdateStatement(), connection as SqlConnection);
            AddParameters(result);
            result.Parameters.AddWithValue(IdentityColumn, DBNull.Value);
            return result;
        }

        private void AddParameters(SqlCommand cmd)
        {
            foreach (var keyPair in this)
            {
                cmd.Parameters.AddWithValue(ParseColumnName(keyPair.Key), keyPair.Value ?? DBNull.Value);
            }
        }

        protected static async Task<string> GetIdentityColumnFromSchema(IDbConnection connection, string schemaName, string tableName)
        {
            return await connection.QuerySingleOrDefaultAsync<string>(
                @"SELECT [col].[name]
                FROM [sys].[columns] [col]
                INNER JOIN [sys].[tables] [t] ON [col].[object_id]=[t].[object_id]
                WHERE 
	                SCHEMA_NAME([t].[schema_id])=@schemaName AND
	                [t].[name]=@tableName AND	                
                    [col].[is_identity]=1", new { schemaName, tableName });
        }

        protected static async Task<IEnumerable<string>> GetColumnsFromSchema(IDbConnection connection, string schemaName, string tableName, IEnumerable<string> keyColumns)
        {
            return await connection.QueryAsync<string>(
                @"SELECT [col].[name]
                FROM [sys].[columns] [col]
                INNER JOIN [sys].[tables] [t] ON [col].[object_id]=[t].[object_id]
                WHERE 
	                SCHEMA_NAME([t].[schema_id])=@schemaName AND
	                [t].[name]=@tableName AND
	                [col].[is_computed]=0 AND
                    [col].[is_identity]=0", new { schemaName, tableName });
        }        

        protected static async Task<IEnumerable<string>> GetKeyColumnsFromSchema(IDbConnection connection, string schemaName, string tableName)
        {
            return await connection.QueryAsync<string>(
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
	                [ndx].[is_primary_key]=1", new { schemaName, tableName });
        }
    }
}
