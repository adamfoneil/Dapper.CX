using AO.Models;
using Dapper.CX.Extensions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace Dapper.CX.Abstract
{
    public abstract class DbDictionary<TKey> : IDbDictionary<TKey>
    {
        private readonly Func<IDbConnection> _getConnection;
        private readonly ObjectName _tableName;

        private bool _initialized = false;

        public DbDictionary(Func<IDbConnection> getConnection, string tableName)
        {
            _getConnection = getConnection;
            _tableName = ObjectName.FromName(tableName);
            TableName = tableName;
        }

        public async Task InitializeAsync()
        {
            if (_initialized) return;

            Dictionary<Type, string> supportedKeyTypes = new Dictionary<Type, string>()
            {
                [typeof(string)] = "nvarchar(255)",
                [typeof(int)] = "int",
                [typeof(long)] = "bigint",
                [typeof(Guid)] = "uniqueidentifier"
            };

            if (!supportedKeyTypes.ContainsKey(typeof(TKey))) throw new Exception($"Unsupported key type {typeof(TKey).Name} used with DbDictionary");

            using (var cn = _getConnection.Invoke())
            {
                if (!await cn.SchemaExistsAsync(_tableName.Schema))
                {
                    await cn.ExecuteAsync($"CREATE SCHEMA [{_tableName.Schema}]");
                }

                if (!await cn.TableExistsAsync(_tableName.Schema, _tableName.Name))
                {
                    await cn.ExecuteAsync(
                        $@"CREATE TABLE [{_tableName.Schema}].[{_tableName.Name}] (
                            [Key] {supportedKeyTypes[typeof(TKey)]} NOT NULL PRIMARY KEY,                            
                            [Value] nvarchar(max) NOT NULL,
                            [DateCreated] datetime NOT NULL,
                            [DateModified] datetime NULL
                        )");
                }
            }

            _initialized = true;
        }

        public string TableName { get; }

        protected abstract TValue Deserialize<TValue>(string value);

        protected abstract string Serialize<TValue>(TValue value);

        public async Task<TValue> GetAsync<TValue>(TKey key, TValue defaultValue = default)
        {
            if (!_initialized) await InitializeAsync();

            var row = await GetRowAsync(key);
            return (row != null) ? Deserialize<TValue>(row.Value) : defaultValue;
        }

        protected async Task<DictionaryRow> GetRowAsync(TKey key)
        {
            using (var cn = _getConnection.Invoke())
            {
                return await cn.QuerySingleOrDefaultAsync<DictionaryRow>($"SELECT * FROM [{_tableName.Schema}].[{_tableName.Name}] WHERE [Key]=@key", new { key });
            }
        }

        public async Task SetAsync<TValue>(TKey key, TValue value)
        {
            if (!_initialized) await InitializeAsync();

            using (var cn = _getConnection.Invoke())
            {
                string json = Serialize(value);

                int affected = await cn.ExecuteAsync($"UPDATE [{_tableName.Schema}].[{_tableName.Name}] SET [Value]=@json, [DateModified]=getutcdate() WHERE [Key]=@key", new { key, json });

                if (affected == 0)
                {
                    await cn.ExecuteAsync($"INSERT INTO [{_tableName.Schema}].[{_tableName.Name}] ([Key], [Value], [DateCreated]) VALUES (@key, @json, getutcdate())", new { key, json });
                }
            }
        }

        public async Task<bool> KeyExistsAsync(TKey key)
        {
            using (var cn = _getConnection.Invoke())
            {
                return await cn.RowExistsAsync($"[{_tableName.Schema}].[{_tableName.Name}] WHERE [Key]=@key", new { key });
            }
        }

        public async Task DeleteAsync(TKey key)
        {
            using (var cn = _getConnection.Invoke())
            {
                await cn.ExecuteAsync($"DELETE [{_tableName.Schema}].[{_tableName.Name}] WHERE [Key]=@key", new { key });
            }
        }

        protected class DictionaryRow
        {
            public TKey Key { get; set; }
            public string Value { get; set; }
            public DateTime DateCreated { get; set; }
            public DateTime? DateModified { get; set; }
        }
    }
}
