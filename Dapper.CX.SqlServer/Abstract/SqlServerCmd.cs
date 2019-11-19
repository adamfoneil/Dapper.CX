using Dapper.CX.Abstract;
using System;
using System.Collections.Generic;
using System.Data;
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
            Initialize(@object);
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

        public override IDbCommand GetInsertCommand(IDbConnection connection)
        {
            throw new NotImplementedException();
        }

        public override IDbCommand GetUpdateCommand(IDbConnection connection)
        {
            throw new NotImplementedException();
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
