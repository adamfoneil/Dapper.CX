using Dapper.CX.Base.Attributes;
using Dapper.CX.Base.Enums;
using Dapper.CX.Base.Exceptions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Dapper.CX.Abstract
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
        /// </summary>
        protected abstract Type[] SupportedTypes { get; }

        protected abstract TIdentity ConvertIdentity(object identity);

        public abstract IDbCommand GetInsertCommand(IDbConnection connection);
        public abstract IDbCommand GetUpdateCommand(IDbConnection connection);

        public string TableName { get; set; }
        public string IdentityColumn { get; set; }
        public bool IdentityInsert { get; set; }
        public TIdentity Id { get; set; }
        public string FormattedTableName() => ApplyDelimiter(TableName);
        public SaveAction SaveAction { get; private set; }

        private Dictionary<string, PropertyInfo> InitializeFromTypeInner(Type type, string[] keyColumns, out PropertyInfo identityProperty)
        {
            var properties = type.GetProperties();

            if (!keyColumns?.Any() ?? true) keyColumns = GetKeyColumns(properties);

            identityProperty = GetIdentityProperty(type, properties);
            IdentityColumn = identityProperty.Name;
            TableName = GetTableName(type);            

            bool isMapped(PropertyInfo pi)
            {
                if (GetColumnName(pi).Equals(IdentityColumn)) return false;
                if (!SupportedTypes.Contains(pi.PropertyType)) return false;

                var attr = pi.GetCustomAttribute<NotMappedAttribute>();
                if (attr != null) return false;

                return true;
            };

            Dictionary<string, PropertyInfo> results = new Dictionary<string, PropertyInfo>();
            foreach (PropertyInfo pi in properties.Where(pi => isMapped(pi)))
            {
                string columnName = GetColumnName(pi, keyColumns);
                Add(columnName, null);
                results.Add(columnName, pi);
            }
            return results;
        }

        protected void InitializeFromType(Type type, params string[] keyColumns)
        {
            InitializeFromTypeInner(type, keyColumns, out PropertyInfo identityProp);
        }

        protected void InitializeFromObject(object @object, params string[] keyColumns)
        {
            var type = @object.GetType();
            var properties = InitializeFromTypeInner(type, keyColumns, out PropertyInfo identityProp);
            Id = ConvertIdentity(identityProp.GetValue(@object));
            foreach (var pi in properties) this[pi.Key] = pi.Value.GetValue(@object);            
        }

        private string[] GetKeyColumns(PropertyInfo[] properties)
        {
            return properties.Where(pi =>
            {
                var attr = pi.GetCustomAttribute<PrimaryKeyAttribute>();
                return (attr != null);
            }).Select(pi => pi.Name).ToArray();
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

        private static string GetColumnName(PropertyInfo propertyInfo)
        {
            string result = propertyInfo.Name;

            var attr = propertyInfo.GetCustomAttribute<ColumnAttribute>();
            if (attr != null) result = attr.Name;

            return result;
        }

        private static string GetColumnName(PropertyInfo propertyInfo, string[] keyColumns)
        {
            string result = GetColumnName(propertyInfo);

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

        protected DynamicParameters GetIdentityParam(TIdentity identityValue)
        {
            var dp = new DynamicParameters();
            dp.Add(IdentityColumn, identityValue);
            return dp;
        }

        public async Task<TModel> GetAsync<TModel>(IDbConnection connection, TIdentity identityValue)
        {
            var result = await connection.QuerySingleOrDefaultAsync<TModel>(SqlQuerySingleStatement(), GetIdentityParam(identityValue));
            if (result != null)
            {
                FillDictionary(identityValue, result);
            }
            return result;
        }

        public async Task<bool> GetAsync(IDbConnection connection, TIdentity identityValue)
        {            
            var result = await connection.QuerySingleOrDefaultAsync(SqlQuerySingleStatement(), GetIdentityParam(identityValue));
            if (result != null)
            {
                FillDictionary(identityValue, result);
                return true;
            }

            return false;
        }

        private void FillDictionary(TIdentity identityValue, object result)
        {
            Id = identityValue;
            var data = ConvertToDictionary(result);
            foreach (var kp in data) Add(kp.Key, kp.Value);
        }

        public bool IsNew()
        {
            return Id.Equals(default(TIdentity));
        }

        public async Task<TIdentity> SaveAsync(IDbConnection connection, Action<SqlCmdDictionary<TIdentity>> onInsert = null, Action<SqlCmdDictionary<TIdentity>> onUpdate = null)
        {            
            if (IsNew())
            {
                onInsert?.Invoke(this);
                Id = await InsertAsync(connection);                
            }
            else
            {
                onUpdate?.Invoke(this);
                await UpdateAsync(connection, Id);                                
            }

            return Id;
        }

        public async Task<TIdentity> InsertAsync(IDbConnection connection)
        {
            try
            {
                var result = await ExecuteInsertAsync(connection);
                SaveAction = SaveAction.Insert;
                return result;
            }
            catch (Exception exc)
            {
                throw new CrudException(this, exc);
            }
        }

        public async Task UpdateAsync(IDbConnection connection, TIdentity identityValue = default)
        {
            try
            {
                if (!identityValue.Equals(default)) Id = identityValue;
                string sql = SqlUpdateStatement();
                var dp = GetParameters();
                dp.Add(IdentityColumn, Id);
                await connection.ExecuteAsync(sql, dp);
                SaveAction = SaveAction.Update;
            }
            catch (Exception exc)
            {
                throw new CrudException(this, exc);
            }
        }

        public async Task<TIdentity> MergeAsync(IDbConnection connection, Action<SqlCmdDictionary<TIdentity>> onInsert = null, Action<SqlCmdDictionary<TIdentity>> onUpdate = null)
        {
            var keyColumns = GetKeyColumnNames();
            if (!keyColumns.Any()) throw new InvalidOperationException("MergeAsync method requires explicit key columns--one or more columns prefixed with '#'.");

            Id = await FindIdentityFromKeyValuesAsync(connection, keyColumns);
            await SaveAsync(connection, onInsert, onUpdate);

            return Id;
        }

        /// <summary>
        /// SQL Server Compact Edition seems to require an explicit transaction when doing an insert and returning the identity value.
        /// This is virtual so that SqlCe can override it with this special behavior.
        /// </summary>
        protected virtual async Task<TIdentity> ExecuteInsertAsync(IDbConnection connection)
        {
            string sql = (!IdentityInsert) ?
                SqlInsertStatement() :
                SqlInsertStatementBase("INSERT");

            if (!IdentityInsert)
            {
                return await connection.QuerySingleOrDefaultAsync<TIdentity>(sql, GetParameters());
            }
            else
            {
                await connection.ExecuteAsync(sql, GetParameters());
                return default;
            }
        }        

        private async Task<TIdentity> FindIdentityFromKeyValuesAsync(IDbConnection connection, IEnumerable<string> keyColumns)
        {
            string query = $"SELECT {ApplyDelimiter(IdentityColumn)} FROM {ApplyDelimiter(TableName)} WHERE {SqlWhereClauseInner(keyColumns)}";
            var dp = new DynamicParameters();
            foreach (var col in keyColumns) dp.Add(col, this[KeyColumnPrefix + col]);
            return await connection.QuerySingleOrDefaultAsync<TIdentity>(query, dp);
        }

        public async Task DeleteAsync(IDbConnection connection, TIdentity identityValue = default)
        {
            if (!identityValue.Equals(default)) Id = identityValue;
            string sql = SqlDeleteStatement();
            await connection.ExecuteAsync(sql, new { id = Id });
        }

        public string SqlDeleteStatement()
        {
            return $"DELETE {FormattedTableName()} WHERE {ApplyDelimiter(IdentityColumn)}=@id";
        }

        public string SqlUpdateStatement()
        {
            // by default, we include all columns in the update
            Func<string, bool> predicate = (key) => true;

            // but if IdentityInsert is on, we need to exclude that from the update
            if (IdentityInsert) predicate = (key) => !key.ToLower().Equals(IdentityColumn.ToLower());

            return
                $@"UPDATE {ApplyDelimiter(TableName)} SET
                    {string.Join(", ", KeysWithValues().Where(kp => predicate(kp)).Select(col => $"{ApplyDelimiter(col)}=@{ParseColumnName(col)}"))}
                WHERE {ApplyDelimiter(IdentityColumn)}=@{IdentityColumn}";
        }

        private string SqlWhereClauseInner(IEnumerable<string> keyColumns)
        {
            return string.Join(" AND ", keyColumns.Select(col => $"{ApplyDelimiter(col)}=@{ParseColumnName(col)}"));
        }

        public string SqlInsertStatement()
        {
            return SqlInsertStatementBase("INSERT") + "; " + SelectIdentityCommand;
        }

        protected string SqlInsertStatementBase(string verb)
        {
            return
                $@"{verb} INTO {ApplyDelimiter(TableName)} (
                    {string.Join(", ", KeysWithValues().Select(s => ApplyDelimiter(s)))}
                ) VALUES (
                    {string.Join(", ", KeysWithValues().Select(s => $"@{ParseColumnName(s)}"))}
                )";
        }

        public string SqlQuerySingleStatement()
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

        private static Dictionary<string, object> ConvertToDictionary(object @object)
        {
            var properties = @object.GetType().GetProperties();
            return properties.ToDictionary(pi => GetColumnName(pi), pi => pi.GetValue(@object));
        }
    }
}
