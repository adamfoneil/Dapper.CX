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
                
                foreach (var kp in this)
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
                    
                    await connection.SaveAsync(history);
                }
            }            
        }

        private Task<int> IncrementRowVersionAsync(IDbConnection connection, string tableName, long rowId, IDbTransaction txn)
        {
            throw new NotImplementedException();
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
