using AO.Models.Interfaces;
using AO.Models.Static;
using Dapper.CX.Extensions;
using Dapper.CX.Interfaces;
using Dapper.CX.Models;
using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Dapper.CX.Classes
{
    public class LoggedChangeTracker<TModel, TIdentity> : ChangeTracker<TModel>, IDbSaveable
    {
        private static bool _initialized = false;

        private readonly IUserBase _user;
        private readonly string _nullText;
        private readonly ISqlCrudProvider<TIdentity> _crudProvider;

        public LoggedChangeTracker(ISqlCrudProvider<TIdentity> crudProvider, IUserBase user, TModel @object, string nullText = "<null>") : base(@object)
        {
            _user = user;
            _nullText = nullText;
            _crudProvider = crudProvider;
        }

        private enum ValueType
        {
            Enum,
            Lookup,
            Raw
        }

        public async Task SaveAsync(IDbConnection connection, IDbTransaction txn)
        {
            string tableName = typeof(TModel).GetTableName();
            long rowId = GetRowId();

            if (txn != null)
            {
                // need to use the existing transaction, and don't commit at the end
                await writeChangeHistoryAsync(txn);
            }
            else
            {
                // you can start your own transaction, and be sure to commit
                using (var innerTxn = connection.BeginTransaction())
                {
                    try
                    {
                        await writeChangeHistoryAsync(innerTxn);
                        innerTxn.Commit();
                    }
                    catch
                    {
                        innerTxn.Rollback();
                        throw;
                    }                    
                }
            }

            async Task writeChangeHistoryAsync(IDbTransaction innerTxn)
            {
                int version = await IncrementRowVersionAsync(connection, tableName, rowId, innerTxn);

                var textLookup = Instance as ITextLookup;

                foreach (var kp in GetModifiedProperties(loggableOnly: true))
                {
                    var rawOldValue = this[kp.Key];
                    var rawNewValue = kp.Value.GetValue(Instance);

                    var valueType =
                        (kp.Value.PropertyType.IsEnum) ? ValueType.Enum :
                        (textLookup?.GetLookupProperties()?.Contains(kp.Key) ?? false) ? ValueType.Lookup :
                        ValueType.Raw;

                    var oldValue =
                        (valueType == ValueType.Enum) ? rawOldValue?.ToString() :
                        (valueType == ValueType.Lookup) ? await textLookup.GetTextFromKeyAsync(connection, innerTxn, kp.Key, rawOldValue) :
                        rawOldValue;

                    var newValue =
                        (valueType == ValueType.Enum) ? rawNewValue?.ToString() :
                        (valueType == ValueType.Lookup) ? await textLookup.GetTextFromKeyAsync(connection, innerTxn, kp.Key, rawNewValue) :
                        rawNewValue;

                    var history = new ColumnHistory()
                    {
                        UserName = _user.Name,
                        Timestamp = _user.LocalTime,
                        TableName = tableName,
                        RowId = rowId,
                        Version = version,
                        ColumnName = kp.Key,
                        OldValue = oldValue?.ToString() ?? _nullText,
                        NewValue = newValue?.ToString() ?? _nullText
                    };

                    await _crudProvider.SaveAsync(connection, history, txn: innerTxn);
                }
            }
        }

        private async Task<int> IncrementRowVersionAsync(IDbConnection connection, string tableName, long rowId, IDbTransaction txn)
        {
            var rowVersion = await _crudProvider.GetWhereAsync<RowVersion>(connection, new { tableName, rowId }, txn) ?? new RowVersion()
            {
                TableName = tableName,
                RowId = rowId
            };

            rowVersion.Version++;

            await _crudProvider.SaveAsync(connection, rowVersion, txn: txn);

            return rowVersion.Version;
        }

        private long GetRowId()
        {
            var idProperty = typeof(TModel).GetIdentityProperty();
            var value = idProperty.GetValue(Instance);
            return Convert.ToInt64(value);
        }

        public static async Task InitializeAsync(IDbConnection connection, ISqlObjectCreator objectCreator)
        {
            if (_initialized) return;

            var statements = await objectCreator.GetStatementsAsync(connection, new Type[]
            {
                typeof(ColumnHistory),
                typeof(RowVersion)
            });

            foreach (var statement in statements)
            {
                await connection.ExecuteAsync(statement);
            }

            _initialized = true;
        }
    }
}
