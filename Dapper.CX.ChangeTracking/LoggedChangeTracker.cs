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

        public LoggedChangeTracker(string userName, TModel @object) : base(@object)
        {
            _userName = userName;
        }

        public async Task SaveAsync(IDbConnection connection)
        {
            await InitializeAsync(connection);

            string tableName = typeof(TModel).GetTableName();
            long rowId = GetRowId();

            using (var txn = connection.BeginTransaction())
            {
                int version = await IncrementRowVersionAsync(connection, tableName, rowId, txn);
                
                foreach (var kp in GetModifiedProperties())
                {
                    var history = new ColumnHistory()
                    {
                        UserName = _userName,
                        Timestamp = DateTime.UtcNow,
                        TableName = tableName,
                        RowId = rowId,
                        Version = version,
                        ColumnName = kp.Key,
                        //OldValue = kp.Value?.ToString(),
                        //NewValue = 
                    };
                    
                    await connection.SaveAsync(history, txn: txn);
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

            //await connection.UpdateAsync(rowVersion, )

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
