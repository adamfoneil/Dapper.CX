using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Dapper.TinyCrud.Abstract
{
    public abstract class SqlCmdDictionary : Dictionary<string, object>
    {
        private const string KeyColumnPrefix = "#";

        protected abstract string SelectIdentityCommand { get; }
        protected abstract char StartDelimiter { get; }
        protected abstract char EndDelimiter { get; }
        protected abstract Dictionary<Type, string> TypeMappings { get; }

        public abstract IDbCommand GetInsertCommand(IDbConnection connection);
        public abstract IDbCommand GetUpdateCommand(IDbConnection connection);

        public string TableName { get; set; }
        public string IdentityColumn { get; set; }
        public bool IdentityInsert { get; set; }
        public string FormattedTableName() => ApplyDelimiter(TableName);
                
        protected static Dictionary<string, object> FromObject(Type[] supportedTypes, object @object, params string[] keyColumns)
        {
            var result = new Dictionary<string, object>();
            var properties = @object.GetType().GetProperties();

            bool isMapped(PropertyInfo pi) 
            {
                if (!supportedTypes.Contains(pi.PropertyType)) return false;

                var attr = pi.GetCustomAttribute<NotMappedAttribute>();
                if (attr != null) return false;

                return true;
            };

            foreach (PropertyInfo pi in properties.Where(pi => isMapped(pi)))
            {
                string columnName = GetColumnName(pi, keyColumns);
                result.Add(columnName, pi.GetValue(@object));
            }

            return result;
        }
        
        private static string GetColumnName(PropertyInfo propertyInfo, string[] keyColumns)
        {
            string result = propertyInfo.Name;

            var attr = propertyInfo.GetCustomAttribute<ColumnAttribute>();
            if (attr != null) result = attr.Name;

            if (keyColumns.Contains(result)) result = KeyColumnPrefix + result;
            return result;
        }

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
            foreach (var kp in this) dp.Add(ParseColumnName(kp.Key), kp.Value);
            return dp;
        }

        public async Task<TIdentity> InsertAsync<TIdentity>(IDbConnection connection)
        {
            return await ExecuteInsertAsync<TIdentity>(connection);
        }

        /// <summary>
        /// SQL Server Compact Edition seems to require an explicit transaction when doing an insert and returning the identity value.
        /// This is virtual so that SqlCe can override it with this special behavior.
        /// </summary>
        protected virtual async Task<TIdentity> ExecuteInsertAsync<TIdentity>(IDbConnection connection)
        {
            string sql = (!IdentityInsert) ?
                GetInsertStatement() :
                GetInsertStatementBase("INSERT");

            if (!IdentityInsert)
            {
                return await connection.QuerySingleAsync<TIdentity>(sql, GetParameters());
            }
            else
            {
                await connection.ExecuteAsync(sql, GetParameters());
                return default(TIdentity);
            }
        }

        public async Task UpdateAsync<TIdentity>(IDbConnection connection, TIdentity identity)
        {
            string sql = GetUpdateStatement();
            var dp = GetParameters();
            dp.Add(IdentityColumn, identity);
            await connection.ExecuteAsync(sql, dp);
        }

        public async Task<TIdentity> MergeAsync<TIdentity>(IDbConnection connection, Action<SqlCmdDictionary> onInsert = null, Action<SqlCmdDictionary> onUpdate = null)
        {
            var keyColumns = GetKeyColumnNames();
            if (!keyColumns.Any()) throw new InvalidOperationException("MergeAsync method requires explicit key columns--one or more columns prefixed with '#'.");

            TIdentity id = await FindIdentityFromKeyValuesAsync<TIdentity>(connection, keyColumns);
            if (id.Equals(default(TIdentity)))
            {
                onInsert?.Invoke(this);
                id = await InsertAsync<TIdentity>(connection);
            }
            else
            {
                onUpdate?.Invoke(this);
                await UpdateAsync(connection, id);
            }

            return id;
        }

        private async Task<TIdentity> FindIdentityFromKeyValuesAsync<TIdentity>(IDbConnection connection, IEnumerable<string> keyColumns)
        {
            string query = $"SELECT {ApplyDelimiter(IdentityColumn)} FROM {ApplyDelimiter(TableName)} WHERE {GetWhereClauseInner(keyColumns)}";
            var dp = new DynamicParameters();
            foreach (var col in keyColumns) dp.Add(col, this[KeyColumnPrefix + col]);
            return await connection.QuerySingleOrDefaultAsync<TIdentity>(query, dp);
        }

        public string GetUpdateStatement()
        {
            // by default, we include all columns in the update
            Func<string, bool> predicate = (key) => true;

            // but if IdentityInsert is on, we need to exclude that from the update
            if (IdentityInsert) predicate = (key) => !key.ToLower().Equals(IdentityColumn.ToLower());

            return
                $@"UPDATE {ApplyDelimiter(TableName)} SET
                    {string.Join(", ", Keys.Where(kp => predicate(kp)).Select(col => $"{ApplyDelimiter(col)}=@{ParseColumnName(col)}"))}
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

        protected string GetInsertStatementBase(string verb)
        {
            return
                $@"{verb} INTO {ApplyDelimiter(TableName)} (
                    {string.Join(", ", Keys.Select(s => ApplyDelimiter(s)))}
                ) VALUES (
                    {string.Join(", ", Keys.Select(s => $"@{ParseColumnName(s)}"))}
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
                if (ContainsKey(col.ColumnName) || ContainsKey("#" + col.ColumnName))
                {
                    this[col.ColumnName] = (!dataRow.IsNull(col)) ? dataRow[col] : null;
                }
            }
        }

    }
}
