﻿using AO.Models.Enums;
using Dapper.CX.Classes;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Dapper.CX.Abstract
{
    public abstract class SqlCmdDictionary : Dictionary<string, object>
    {
        protected const string KeyColumnPrefix = "#";

        public SqlCmdDictionary()
        {
        }

        public SqlCmdDictionary(IEnumerable<string> columnNames)
        {
            if (columnNames != null)
            {
                foreach (var name in columnNames) Add(name, null);
            }
        }

        public SqlCmdDictionary(string tableName, IEnumerable<string> columnNames = null) : this(columnNames)
        {
            TableName = tableName;
            IdentityColumn = null;
            IdentityInsert = true;
        }

        public SqlCmdDictionary(string tableName, string identityColumn, IEnumerable<string> columnNames = null) : this(columnNames)
        {
            TableName = tableName;
            IdentityColumn = identityColumn;
        }

        public string TableName { get; set; }
        public string IdentityColumn { get; set; }
        public bool IdentityInsert { get; set; }
        public string FormattedTableName() => ApplyDelimiter(TableName);

        protected abstract string SelectIdentityCommand { get; }
        protected abstract char StartDelimiter { get; }
        protected abstract char EndDelimiter { get; }

        public abstract IDbCommand GetInsertCommand(IDbConnection connection);
        public abstract IDbCommand GetUpdateCommand(IDbConnection connection);

        public new object this[string columnName]
        {
            get { return (ContainsKey(KeyColumnPrefix + columnName)) ? base[KeyColumnPrefix + columnName] : base[columnName]; }
            set
            {
                if (ContainsKey(KeyColumnPrefix + columnName))
                {
                    base[KeyColumnPrefix + columnName] = value;
                }
                else
                {
                    base[columnName] = value;
                }
            }
        }

        protected string ApplyDelimiter(string name)
        {
            return string.Join(".", name.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries).Select(s => $"{StartDelimiter}{ParseColumnName(s)}{EndDelimiter}"));
        }

        protected IEnumerable<string> GetKeyColumnNames()
        {
            return Keys.Where(key => IsKeyColumn(key)).Select(key => ParseColumnName(key));
        }

        protected IEnumerable<string> GetNonKeyColumnNames()
        {
            return Keys.Where(key => !IsKeyColumn(key)).Select(key => key);
        }

        private bool IsKeyColumn(string columnName)
        {
            return columnName.StartsWith(KeyColumnPrefix);
        }

        /// <summary>
        /// Key columns are prefixed with a # symbol
        /// </summary>
        protected string ParseColumnName(string columnName)
        {
            string result = columnName;
            if (result.StartsWith(KeyColumnPrefix)) result = columnName.Substring(KeyColumnPrefix.Length).Trim();
            return result;
        }

        protected DynamicParameters GetParameters(object template = null)
        {
            var dp = new DynamicParameters(template);
            foreach (var kp in this.Where(kp => !(kp.Value is SqlExpression))) dp.Add(ParseColumnName(kp.Key), kp.Value);
            return dp;
        }

        public async Task<TIdentity> InsertAsync<TIdentity>(IDbConnection connection, IDbTransaction txn = null) => 
            await ExecuteInsertAsync<TIdentity>(connection, txn);

        public async Task InsertAsync(IDbConnection connection, IDbTransaction txn = null)
        {
            IdentityInsert = true;
            await ExecuteInsertAsync<int>(connection, txn);
        }

        /// <summary>
        /// SQL Server Compact Edition seems to require an explicit transaction when doing an insert and returning the identity value.
        /// This is virtual so that SqlCe can override it with this special behavior.
        /// </summary>
        protected virtual async Task<TIdentity> ExecuteInsertAsync<TIdentity>(IDbConnection connection, IDbTransaction txn = null)
        {
            string sql = (!IdentityInsert) ?
                GetInsertStatement() :
                GetInsertStatementBase("INSERT");

            if (!IdentityInsert)
            {
                return await connection.QuerySingleAsync<TIdentity>(sql, GetParameters(), txn);
            }
            else
            {
                await connection.ExecuteAsync(sql, GetParameters(), txn);
                return default;
            }
        }

        public async Task UpdateAsync<TIdentity>(IDbConnection connection, TIdentity identity, IDbTransaction txn = null)
        {
            string sql = GetUpdateStatement();
            var dp = GetParameters();
            dp.Add(IdentityColumn, identity);
            await connection.ExecuteAsync(sql, dp, txn);
        }

        public async Task<TIdentity> MergeAsync<TIdentity>(IDbConnection connection, Action<SqlCmdDictionary, SaveAction> onSave = null, IDbTransaction txn = null)
        {
            var keyColumns = GetKeyColumnNames();
            if (!keyColumns.Any()) throw new InvalidOperationException("MergeAsync method requires explicit key columns--one or more columns prefixed with '#'.");

            TIdentity id = await FindIdentityFromKeyValuesAsync<TIdentity>(connection, keyColumns, txn);
            if (id.Equals(default(TIdentity)))
            {
                onSave?.Invoke(this, SaveAction.Insert);
                id = await InsertAsync<TIdentity>(connection, txn);
            }
            else
            {
                onSave?.Invoke(this, SaveAction.Update);
                await UpdateAsync(connection, id, txn);
            }

            return id;
        }

        private async Task<TIdentity> FindIdentityFromKeyValuesAsync<TIdentity>(IDbConnection connection, IEnumerable<string> keyColumns, IDbTransaction txn = null)
        {
            string query = $"SELECT {ApplyDelimiter(IdentityColumn)} FROM {ApplyDelimiter(TableName)} WHERE {GetWhereClauseInner(keyColumns)}";
            var dp = new DynamicParameters();
            foreach (var col in keyColumns) dp.Add(col, this["#" + col]);
            return await connection.QuerySingleOrDefaultAsync<TIdentity>(query, dp, txn);
        }

        public string GetUpdateStatement()
        {
            // by default, we include all columns in the update
            Func<KeyValuePair<string, object>, bool> predicate = (key) => true;

            // but if IdentityInsert is on, we need to exclude that from the update
            if (IdentityInsert) predicate = (kp) => !kp.Key.ToLower().Equals(IdentityColumn.ToLower());

            return
                $@"UPDATE {ApplyDelimiter(TableName)} SET
                    {string.Join(", ", this.Where(kp => predicate(kp)).Select(col => $"{ApplyDelimiter(col.Key)}={ValueSyntax(col)}"))}
                WHERE {ApplyDelimiter(IdentityColumn)}=@{IdentityColumn}";
        }

        private string GetWhereClauseInner(IEnumerable<string> keyColumns)
        {
            return string.Join(" AND ", keyColumns.Select(col => $"{ApplyDelimiter(col)}=@{ParseColumnName(col)}"));
        }

        public string GetInsertStatement()
        {
            return GetInsertStatementBase("INSERT") + "; " + SelectIdentityCommand;
        }

        protected string ValueSyntax(KeyValuePair<string, object> keyPair)
        {
            return (keyPair.Value is SqlExpression) ?
                (keyPair.Value as SqlExpression).Content :
                $"@{ParseColumnName(keyPair.Key)}";
        }

        protected string GetInsertStatementBase(string verb)
        {
            return
                $@"{verb} INTO {ApplyDelimiter(TableName)} (
                    {string.Join(", ", Keys.Select(s => ApplyDelimiter(s)))}
                ) VALUES (
                    {string.Join(", ", this.Select(kp => ValueSyntax(kp)))}
                )";
        }

        protected string GetFindStatement()
        {
            return $"SELECT {string.Join(", ", Keys.Select(key => ApplyDelimiter(key)))} FROM {ApplyDelimiter(TableName)} WHERE {ApplyDelimiter(IdentityColumn)}=@{IdentityColumn}";
        }

        /// <summary>
        /// Associates values in a DataRow with values of this dictionary 
        /// </summary>        
        public void BindDataRow(DataRow dataRow)
        {
            foreach (DataColumn col in dataRow.Table.Columns)
            {
                if (ContainsKey(col.ColumnName) || ContainsKey(KeyColumnPrefix + col.ColumnName))
                {
                    this[col.ColumnName] = (!dataRow.IsNull(col)) ? dataRow[col] : null;
                }
            }
        }
    }
}
