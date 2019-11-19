using Dapper.TinyCrud.Base.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Dapper.TinyCrud.Abstract
{
    public abstract class SqlCmdDictionary<TIdentity> : Dictionary<string, object>
    {
        private const string KeyColumnPrefix = "#";
        public const string DefaultIdentityProperty = "Id";

        protected abstract string SelectIdentityCommand { get; }
        protected abstract char StartDelimiter { get; }
        protected abstract char EndDelimiter { get; }

        /// <summary>
        /// Types supported by this SqlCmdDictionary when mapping to an object.
        /// Don't include nullable versions since they are derived automatically
        /// </summary>
        protected abstract Type[] SupportedTypes { get; }

        protected abstract TIdentity ConvertIdentity(object identity);

        public abstract IDbCommand GetInsertCommand(IDbConnection connection);
        public abstract IDbCommand GetUpdateCommand(IDbConnection connection);

        public string TableName { get; set; }
        public string IdentityColumn { get; set; }
        public bool IdentityInsert { get; set; }        
        public TIdentity IdentityValue { get; set; }
        public string FormattedTableName() => ApplyDelimiter(TableName);
                
        protected void Initialize(object @object, params string[] keyColumns)
        {
            var type = @object.GetType();
            var properties = type.GetProperties();

            if (!keyColumns?.Any() ?? true) keyColumns = GetKeyColumns(properties);

            var identityProp = GetIdentityProperty(type, properties);
            IdentityColumn = identityProp.Name;
            TableName = GetTableName(type);
            IdentityValue = ConvertIdentity(identityProp.GetValue(@object));
                        
            var allSupportedTypes = SupportedTypes.Concat(ToNullable(SupportedTypes));

            bool isMapped(PropertyInfo pi) 
            {
                if (!allSupportedTypes.Contains(pi.PropertyType)) return false;

                var attr = pi.GetCustomAttribute<NotMappedAttribute>();
                if (attr != null) return false;

                return true;
            };

            foreach (PropertyInfo pi in properties.Where(pi => isMapped(pi)))
            {
                string columnName = GetColumnName(pi, keyColumns);
                Add(columnName, pi.GetValue(@object));
            }     
        }

        private string[] GetKeyColumns(PropertyInfo[] properties)
        {
            throw new NotImplementedException();
        }

        private string GetTableName(Type type)
        {
            string result = type.Name;

            var attr = type.GetCustomAttribute<TableAttribute>();
            if (attr != null)
            {
                result = attr.Name;
                if (!string.IsNullOrEmpty(attr.Schema)) result = attr.Schema + "." + attr.Name;
            }

            return result;
        }

        private PropertyInfo GetIdentityProperty(Type type, PropertyInfo[] properties)
        {
            var attr = type.GetCustomAttribute<IdentityAttribute>();

            var propertyDictionary = properties.ToDictionary(pi => pi.Name);            

            return
                (attr != null && propertyDictionary.ContainsKey(attr.ColumnName)) ? propertyDictionary[attr.ColumnName] :
                (propertyDictionary.ContainsKey(DefaultIdentityProperty)) ? propertyDictionary[DefaultIdentityProperty] :
                throw new Exception($"Couldn't determine identity property of type {type.Name}");
        }

        private static IEnumerable<Type> ToNullable(Type[] types)
        {
            return types.Select(t => t.MakeGenericType(typeof(Nullable<>), t));
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

        public async Task<TIdentity> SaveAsync(IDbConnection connection, Action<SqlCmdDictionary<TIdentity>> onInsert = null, Action<SqlCmdDictionary<TIdentity>> onUpdate = null)
        {            
            if (IdentityValue.Equals(default(TIdentity)))
            {
                onInsert?.Invoke(this);
                IdentityValue = await InsertAsync(connection);                
            }
            else
            {
                onUpdate?.Invoke(this);
                await UpdateAsync(connection, IdentityValue);                                
            }

            return IdentityValue;
        }

        public async Task<TIdentity> InsertAsync(IDbConnection connection)
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

        public async Task UpdateAsync(IDbConnection connection, TIdentity identityValue = default)
        {
            if (!identityValue.Equals(default)) IdentityValue = identityValue;
            string sql = GetUpdateStatement();
            var dp = GetParameters();
            dp.Add(IdentityColumn, IdentityValue);
            await connection.ExecuteAsync(sql, dp);
        }

        public async Task<TIdentity> MergeAsync(IDbConnection connection, Action<SqlCmdDictionary<TIdentity>> onInsert = null, Action<SqlCmdDictionary<TIdentity>> onUpdate = null)
        {
            var keyColumns = GetKeyColumnNames();
            if (!keyColumns.Any()) throw new InvalidOperationException("MergeAsync method requires explicit key columns--one or more columns prefixed with '#'.");

            IdentityValue = await FindIdentityFromKeyValuesAsync(connection, keyColumns);            
            await SaveAsync(connection, onInsert, onUpdate);

            return IdentityValue;
        }

        private async Task<TIdentity> FindIdentityFromKeyValuesAsync(IDbConnection connection, IEnumerable<string> keyColumns)
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
                if (ContainsKey(col.ColumnName) || ContainsKey(KeyColumnPrefix + col.ColumnName))
                {
                    this[col.ColumnName] = (!dataRow.IsNull(col)) ? dataRow[col] : null;
                }
            }
        }
    }
}
