using AO.DbSchema.Attributes.Interfaces;
using Dapper.CX.ChangeTracking.Models;
using Dapper.CX.Extensions;
using Dapper.CX.Interfaces;
using Dapper.CX.SqlServer.Extensions.Long;
using ModelSync.Library.Models;
using System;
using System.Data;
using System.Threading.Tasks;

namespace Dapper.CX.Classes
{
    public class LoggedChangeTracker<TModel> : ChangeTracker<TModel>, IDbSaveable
    {
        private static bool _initialized = false;

        private readonly string _userName;
        private readonly string _nullText;

        public LoggedChangeTracker(string userName, TModel @object, string nullText = "<null>") : base(@object)
        {
            _userName = userName;
            _nullText = nullText;
        }

        public async Task SaveAsync(IDbConnection connection)
        {
            await InitializeAsync(connection);

            string tableName = typeof(TModel).GetTableName();
            long rowId = GetRowId();

            using (var txn = connection.BeginTransaction())
            {
                try
                {
                    int version = await IncrementRowVersionAsync(connection, tableName, rowId, txn);

                    var textLookup = Instance as ITextLookup;

                    foreach (var kp in GetModifiedProperties())
                    {
                        var oldValue = this[kp.Key];
                        var newValue = kp.Value.GetValue(Instance);

                        var history = new ColumnHistory()
                        {
                            UserName = _userName,
                            Timestamp = DateTime.UtcNow,
                            TableName = tableName,
                            RowId = rowId,
                            Version = version,
                            ColumnName = kp.Key,
                            OldValue = await textLookup?.GetTextFromKeyAsync(connection, kp.Key, oldValue) ?? oldValue?.ToString() ?? _nullText,
                            NewValue = await textLookup?.GetTextFromKeyAsync(connection, kp.Key, newValue) ?? newValue?.ToString() ?? _nullText
                        };

                        await connection.SaveAsync(history, txn: txn);
                    }

                    txn.Commit();
                }
                catch 
                {
                    txn.Rollback();
                    throw;
                }
            }            
        }

        private async Task<int> IncrementRowVersionAsync(IDbConnection connection, string tableName, long rowId, IDbTransaction txn)
        {
            var rowVersion = await connection.GetWhereAsync<RowVersion>(new { tableName, rowId }, txn) ?? new RowVersion()
            {
                TableName = tableName,
                RowId = rowId                
            };

            rowVersion.Version++;

            await connection.UpdateAsync(rowVersion, txn, r => r.Version);

            return rowVersion.Version;
        }

        private long GetRowId()
        {
            var idProperty = typeof(TModel).GetIdentityProperty();
            var value = idProperty.GetValue(Instance);
            return Convert.ToInt64(value);
        }

        private async Task InitializeAsync(IDbConnection connection)
        {
            if (_initialized) return;

            await DataModel.CreateTablesAsync(new Type[]
            {
                typeof(ColumnHistory),
                typeof(RowVersion)
            }, connection);

            _initialized = true;
        }
    }
}
