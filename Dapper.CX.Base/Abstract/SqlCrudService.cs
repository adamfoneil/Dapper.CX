using AO.Models.Enums;
using AO.Models.Interfaces;
using AO.Models.Static;
using Dapper.CX.Classes;
using Dapper.CX.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Dapper.CX.Abstract
{
    public abstract class SqlCrudService<TIdentity, TUser> : ISqlCrudService<TIdentity, TUser> where TUser : IUserBase
    {
        public readonly SqlCrudProvider<TIdentity> CrudProvider;
        protected readonly string _connectionString;

        public SqlCrudService(string connectionString, TUser user, SqlCrudProvider<TIdentity> crudProvider)
        {
            _connectionString = connectionString;
            CrudProvider = crudProvider;
            User = user;
        }

        public abstract IDbConnection GetConnection();

        public TUser User { get; }

        public bool HasUser => !string.IsNullOrEmpty(User?.Name);

        public async Task UpdateUserAsync(params Expression<Func<TUser, object>>[] setColumns)
        {
            if (User is SystemUser) throw new Exception("Can't update a SystemUser account.");

            using (var cn = GetConnection())
            {
                await CrudProvider.UpdateAsync(cn, User, setColumns);
                if (OnUserUpdatedAsync != null)
                {
                    await OnUserUpdatedAsync.Invoke(User);
                }
            }
        }

        public Func<TUser, Task> OnUserUpdatedAsync { get; set; }

        public async Task<TModel> GetAsync<TModel>(IDbConnection connection, TIdentity id)
        {
            return await CrudProvider.GetAsync<TModel>(connection, id, user: User);
        }

        public async Task<TModel> GetAsync<TModel>(TIdentity id)
        {
            using (var cn = GetConnection())
            {
                return await GetAsync<TModel>(cn, id);
            }
        }

        public async Task<TModel> GetWhereAsync<TModel>(IDbConnection connection, object criteria, IDbTransaction txn = null)
        {
            return await CrudProvider.GetWhereAsync<TModel>(connection, criteria, txn: txn, user: User);
        }

        public async Task<TModel> GetWhereAsync<TModel>(IDbConnection connection, params Expression<Func<TModel, bool>>[] criteria)
        {
            return await CrudProvider.GetWhereAsync(connection, User, null, criteria);
        }

        public async Task<TModel> GetWhereAsync<TModel>(params Expression<Func<TModel, bool>>[] criteria)
        {
            using (var cn = GetConnection())
            {
                return await GetWhereAsync(cn, criteria);
            }
        }

        public async Task<TModel> GetWhereAsync<TModel>(object criteria, IDbTransaction txn = null)
        {
            using (var cn = GetConnection())
            {
                return await GetWhereAsync<TModel>(cn, criteria, txn: txn);
            }
        }

        public async Task<bool> ExistsAsync<TModel>(IDbConnection connection, TIdentity id)
        {
            return await CrudProvider.ExistsAsync<TModel>(connection, id);
        }

        public async Task<bool> ExistsAsync<TModel>(TIdentity id)
        {
            using (var cn = GetConnection())
            {
                return await ExistsAsync<TModel>(cn, id);
            }
        }

        public async Task<bool> ExistsWhereAsync<TModel>(IDbConnection connection, object criteria)
        {
            return await CrudProvider.ExistsWhereAsync<TModel>(connection, criteria);
        }

        public async Task<bool> ExistsWhereAsync<TModel>(object criteria)
        {
            using (var cn = GetConnection())
            {
                return await ExistsWhereAsync<TModel>(cn, criteria);
            }
        }

        public async Task<TIdentity> SaveAsync<TModel>(IDbConnection connection, TModel model, string[] columnNames)
        {
            return await CrudProvider.SaveAsync(connection, model, columnNames);
        }

        public async Task<TIdentity> SaveAsync<TModel>(TModel model, string[] columnNames)
        {
            using (var cn = GetConnection())
            {
                return await SaveAsync(cn, model, columnNames);
            }
        }

        public async Task<TIdentity> SaveAsync<TModel>(
            IDbConnection connection,
            TModel model, ChangeTracker<TModel> changeTracker = null,
            Func<IDbConnection, IDbTransaction, Task> txnAction = null, Action<SaveAction, TModel> onSave = null)
        {
            return await ExecuteInnerAsync<TModel>(connection, (cn, txn) => CrudProvider.SaveAsync(cn, model, changeTracker, txn, User, onSave), txnAction);
        }

        public async Task<TIdentity> SaveAsync<TModel>(
            TModel model, ChangeTracker<TModel> changeTracker = null,
            Func<IDbConnection, IDbTransaction, Task> txnAction = null, Action<SaveAction, TModel> onSave = null)
        {
            using (var connection = GetConnection())
            {
                return await SaveAsync(connection, model, changeTracker, txnAction, onSave);                
            }
        }

        public async Task<TIdentity> MergeAsync<TModel>(
            IDbConnection connection,
            TModel model, ChangeTracker<TModel> changeTracker = null,
            Func<IDbConnection, IDbTransaction, Task> txnAction = null, Action<SaveAction, TModel> onSave = null)
        {
            return await ExecuteInnerAsync<TModel>(connection, (cn, txn) => CrudProvider.MergeAsync(cn, model, changeTracker, txn, User, onSave), txnAction);
        }

        public async Task<TIdentity> MergeAsync<TModel>(
            TModel model, ChangeTracker<TModel> changeTracker = null,
            Func<IDbConnection, IDbTransaction, Task> txnAction = null, Action<SaveAction, TModel> onSave = null)
        {
            using (var connection = GetConnection())
            {
                return await ExecuteInnerAsync<TModel>(connection, (cn, txn) => CrudProvider.MergeAsync(cn, model, changeTracker, txn, User, onSave), txnAction);
            }
        }

        public async Task<TIdentity> InsertAsync<TModel>(
            IDbConnection connection,
            TModel model, Func<IDbConnection, IDbTransaction, Task> txnAction = null)
        {
            return await ExecuteInnerAsync<TModel>(connection, (cn, txn) => CrudProvider.InsertAsync(cn, model, getIdentity: true, txn, User), txnAction);
        }

        public async Task<TIdentity> InsertAsync<TModel>(
            TModel model, Func<IDbConnection, IDbTransaction, Task> txnAction = null)
        {
            using (var connection = GetConnection())
            {
                return await ExecuteInnerAsync<TModel>(connection, (cn, txn) => CrudProvider.InsertAsync(cn, model, getIdentity: true, txn, User), txnAction);
            }
        }

        public async Task UpdateAsync<TModel>(
            IDbConnection connection,
            TModel model, ChangeTracker<TModel> changeTracker = null,
            Func<IDbConnection, IDbTransaction, Task> txnAction = null)
        {
            await ExecuteInnerAsync<TModel>(connection, async (cn, txn) =>
            {
                await CrudProvider.UpdateAsync(cn, model, changeTracker, txn, User);
                return default;
            }, txnAction);
        }

        public async Task UpdateAsync<TModel>(
            TModel model, ChangeTracker<TModel> changeTracker = null,
            Func<IDbConnection, IDbTransaction, Task> txnAction = null)
        {
            using (var connection = GetConnection())
            {
                await ExecuteInnerAsync<TModel>(connection, async (cn, txn) =>
                {
                    await CrudProvider.UpdateAsync(cn, model, changeTracker, txn, User);
                    return default;
                }, txnAction);
            }
        }

        public async Task DeleteAsync<TModel>(
            IDbConnection connection,
            TModel model, Func<IDbConnection, IDbTransaction, Task> txnAction = null)
        {
            await ExecuteInnerAsync<TModel>(connection, async (cn, txn) =>
            {
                await CrudProvider.DeleteAsync(cn, model, txn, User);
                return default;
            }, txnAction);
        }

        public async Task DeleteAsync<TModel>(
            TModel model, Func<IDbConnection, IDbTransaction, Task> txnAction = null)
        {
            using (var connection = GetConnection())
            {
                await ExecuteInnerAsync<TModel>(connection, async (cn, txn) =>
                {
                    await CrudProvider.DeleteAsync(cn, model, txn, User);
                    return default;
                }, txnAction);
            }
        }

        public async Task DeleteAsync<TModel>(
            IDbConnection connection,
            TIdentity id, Func<IDbConnection, IDbTransaction, Task> txnAction = null)
        {
            await ExecuteInnerAsync<TModel>(connection, async (cn, txn) =>
            {
                await CrudProvider.DeleteAsync<TModel>(cn, id, txn, User);
                return default;
            }, txnAction);
        }

        public async Task DeleteAsync<TModel>(
            TIdentity id, Func<IDbConnection, IDbTransaction, Task> txnAction = null)
        {
            using (var cn = GetConnection())
            {
                await DeleteAsync<TModel>(cn, id, txnAction);
            }
        }

        /// <summary>
        /// query any table with an object-based WHERE clause.
        /// This is meant for small tables to populate dropdowns
        /// </summary>
        public async Task<IEnumerable<TResult>> QueryAsync<TResult>(object criteria = null)
        {
            var sql = (criteria != null) ?
                CrudProvider.GetQuerySingleWhereStatement(typeof(TResult), criteria) :
                GetQueryStatement(typeof(TResult));

            using (var cn = GetConnection())
            {
                return await cn.QueryAsync<TResult>(sql, criteria);
            }

            string GetQueryStatement(Type type) => $"SELECT * FROM {SqlBuilder.ApplyDelimiter(type.GetTableName(), '[', ']')}";
        }        

        private async Task<TIdentity> ExecuteInnerAsync<TModel>(
            IDbConnection connection,
            Func<IDbConnection, IDbTransaction, Task<TIdentity>> crudAction,
            Func<IDbConnection, IDbTransaction, Task> txnAction = null)
        {
            if (connection.State == ConnectionState.Closed) connection.Open();
            using (var txn = connection.BeginTransaction())
            {
                try
                {
                    var result = await crudAction.Invoke(connection, txn);
                    if (txnAction != null) await txnAction.Invoke(connection, txn);
                    txn.Commit();
                    return result;
                }
                catch
                {
                    txn.Rollback();
                    throw;
                }
            }
        }

        #region Try methods
        public async Task<bool> TryUpdateUserAsync(Func<Task> onSuccess = null, Action<Exception> onException = null)
        {
            try
            {
                await UpdateUserAsync();
                if (onSuccess != null) await onSuccess.Invoke();
                return true;
            }
            catch (Exception exc)
            {
                if (onException != null) onException.Invoke(exc);
                return false;
            }
        }

        public async Task<TIdentity> TrySaveAsync<TModel>(
            IDbConnection connection,
            TModel model, ChangeTracker<TModel> changeTracker = null,
            Func<TIdentity, Task> onSuccess = null, Action<Exception> onException = null, Action<SaveAction, TModel> onSave = null)
        {
            try
            {
                var result = await SaveAsync(connection, model, changeTracker, onSave: onSave);
                if (onSuccess != null) await onSuccess.Invoke(result);
                return result;
            }
            catch (Exception exc)
            {
                if (onException != null) onException.Invoke(exc);
                return CrudProvider.GetIdentity(model);
            }
        }

        public async Task<TIdentity> TrySaveAsync<TModel>(
            TModel model, ChangeTracker<TModel> changeTracker = null,
            Func<TIdentity, Task> onSuccess = null, Action<Exception> onException = null, Action<SaveAction, TModel> onSave = null)
        {
            using (var cn = GetConnection())
            {
                return await TrySaveAsync(cn, model, changeTracker, onSuccess, onException, onSave);
            }
        }

        public async Task<bool> TryMergeAsync<TModel>(
            IDbConnection connection,
            TModel model, ChangeTracker<TModel> changeTracker = null,
            Func<TIdentity, Task> onSuccess = null, Action<Exception> onException = null, Action<SaveAction, TModel> onSave = null)
        {
            try
            {
                var result = await MergeAsync(connection, model, changeTracker, onSave: onSave);
                if (onSuccess != null) await onSuccess.Invoke(result);
                return true;
            }
            catch (Exception exc)
            {
                if (onException != null) onException.Invoke(exc);
                return false;
            }
        }

        public async Task<bool> TryMergeAsync<TModel>(
            TModel model, ChangeTracker<TModel> changeTracker = null,
            Func<TIdentity, Task> onSuccess = null, Action<Exception> onException = null, Action<SaveAction, TModel> onSave = null)
        {
            using (var cn = GetConnection())
            {
                return await TryMergeAsync(cn, model, changeTracker, onSuccess, onException, onSave);
            }
        }

        public async Task<bool> TryInsertAsync<TModel>(
            IDbConnection connection,
            TModel model, Func<TIdentity, Task> onSuccess = null, Action<Exception> onException = null)
        {
            try
            {
                var result = await InsertAsync(connection, model);
                if (onSuccess != null) await onSuccess.Invoke(result);
                return true;
            }
            catch (Exception exc)
            {
                if (onException != null) onException.Invoke(exc);
                return false;
            }
        }

        public async Task<bool> TryInsertAsync<TModel>(TModel model, Func<TIdentity, Task> onSuccess = null, Action<Exception> onException = null)
        {
            using (var cn = GetConnection())
            {
                return await TryInsertAsync(cn, model, onSuccess, onException);
            }
        }

        public async Task<bool> TryDeleteAsync<TModel>(IDbConnection connection, TIdentity id, Func<Task> onSuccess = null, Action<Exception> onException = null)
        {
            try
            {
                await DeleteAsync<TModel>(connection, id);
                if (onSuccess != null) await onSuccess.Invoke();
                return true;
            }
            catch (Exception exc)
            {
                if (onException != null) onException.Invoke(exc);
                return false;
            }
        }

        public async Task<bool> TryDeleteAsync<TModel>(TIdentity id, Func<Task> onSuccess = null, Action<Exception> onException = null)
        {
            using (var cn = GetConnection())
            {
                return await TryDeleteAsync<TModel>(cn, id, onSuccess, onException);
            }
        }

        public async Task<bool> TryDeleteAsync<TModel>(IDbConnection connection, TModel model, Func<Task> onSuccess = null, Action<Exception> onException = null)
        {
            try
            {
                await DeleteAsync(connection, model);
                if (onSuccess != null) await onSuccess.Invoke();
                return true;
            }
            catch (Exception exc)
            {
                if (onException != null) onException.Invoke(exc);
                return false;
            }
        }

        public async Task<bool> TryDeleteAsync<TModel>(TModel model, Func<Task> onSuccess = null, Action<Exception> onException = null)
        {
            using (var cn = GetConnection())
            {
                return await TryDeleteAsync(cn, model, onSuccess, onException);
            }
        }

        public async Task<bool> TryUpdateAsync<TModel>(
            IDbConnection connection,
            TModel model, ChangeTracker<TModel> changeTracker = null,
            Func<Task> onSuccess = null, Action<Exception> onException = null)
        {
            try
            {
                await UpdateAsync(connection, model, changeTracker);
                if (onSuccess != null) await onSuccess.Invoke();
                return true;
            }
            catch (Exception exc)
            {
                if (onException != null) onException.Invoke(exc);
                return false;
            }
        }

        public async Task<bool> TryUpdateAsync<TModel>(
            TModel model, ChangeTracker<TModel> changeTracker = null,
            Func<Task> onSuccess = null, Action<Exception> onException = null)
        {
            using (var cn = GetConnection())
            {
                return await TryUpdateAsync(cn, model, changeTracker, onSuccess, onException);
            }
        }
        #endregion
    }
}
