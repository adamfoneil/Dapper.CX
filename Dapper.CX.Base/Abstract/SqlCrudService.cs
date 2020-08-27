using AO.Models.Interfaces;
using Dapper.CX.Classes;
using System;
using System.Data;
using System.Threading.Tasks;

namespace Dapper.CX.Abstract
{
    public abstract class SqlCrudService<TIdentity, TUser> where TUser : IUserBase
    {
        protected readonly SqlCrudProvider<TIdentity> CrudProvider;
        protected readonly string _connectionString;

        public SqlCrudService(string connectionString, TUser user, SqlCrudProvider<TIdentity> crudProvider)
        {
            _connectionString = connectionString;
            CrudProvider = crudProvider;
            User = user;           
        }
        
        public abstract IDbConnection GetConnection();
        
        public TUser User { get; }

        public bool HasUser => User != null;

        public async Task UpdateUserAsync()
        {
            using (var cn = GetConnection())
            {
                await CrudProvider.UpdateAsync(cn, User);
            }
        }

        public async Task<TModel> GetAsync<TModel>(TIdentity id)
        {
            using (var cn = GetConnection())
            {
                return await CrudProvider.GetAsync<TModel>(cn, id, user: User);
            }
        }

        public async Task<TModel> GetWhereAsync<TModel>(object criteria)
        {
            using (var cn = GetConnection())
            {
                return await CrudProvider.GetWhereAsync<TModel>(cn, criteria, user: User);
            }
        }

        public async Task<bool> ExistsAsync<TModel>(TIdentity id)
        {
            using (var cn = GetConnection())
            {
                return await CrudProvider.ExistsAsync<TModel>(cn, id);
            }
        }

        public async Task<bool> ExistsWhereAsync<TModel>(object criteria)
        {
            using (var cn = GetConnection())
            {
                return await CrudProvider.ExistsWhereAsync<TModel>(cn, criteria);
            }
        }

        public async Task<TIdentity> SaveAsync<TModel>(TModel model, string[] columnNames)
        {
            using (var cn = GetConnection())
            {
                return await CrudProvider.SaveAsync(cn, model, columnNames);
            }
        }

        public async Task<TIdentity> SaveAsync<TModel>(
            TModel model, ChangeTracker<TModel> changeTracker = null,
            Func<IDbConnection, IDbTransaction, Task> txnAction = null)
        {
            return await ExecuteInnerAsync<TModel>((cn, txn) => CrudProvider.SaveAsync(cn, model, changeTracker, txn, User), txnAction);
        }

        public async Task<TIdentity> MergeAsync<TModel>(
            TModel model, ChangeTracker<TModel> changeTracker = null,
            Func<IDbConnection, IDbTransaction, Task> txnAction = null)
        {
            return await ExecuteInnerAsync<TModel>((cn, txn) => CrudProvider.MergeAsync(cn, model, changeTracker, txn, User), txnAction);
        }

        public async Task<TIdentity> InsertAsync<TModel>(
            TModel model, Func<IDbConnection, IDbTransaction, Task> txnAction = null)
        {
            return await ExecuteInnerAsync<TModel>((cn, txn) => CrudProvider.InsertAsync(cn, model, getIdentity: true, txn, User), txnAction);
        }

        public async Task UpdateAsync<TModel>(
            TModel model, ChangeTracker<TModel> changeTracker = null,
            Func<IDbConnection, IDbTransaction, Task> txnAction = null)
        {
            await ExecuteInnerAsync<TModel>(async (cn, txn) =>
            {
                await CrudProvider.UpdateAsync(cn, model, changeTracker, txn, User);
                return default;
            }, txnAction);
        }

        public async Task DeleteAsync<TModel>(
            TModel model, Func<IDbConnection, IDbTransaction, Task> txnAction = null)
        {
            await ExecuteInnerAsync<TModel>(async (cn, txn) =>
            {
                await CrudProvider.DeleteAsync(cn, model, txn, User);
                return default;
            }, txnAction);
        }

        public async Task DeleteAsync<TModel>(
            TIdentity id, Func<IDbConnection, IDbTransaction, Task> txnAction = null)
        {
            await ExecuteInnerAsync<TModel>(async (cn, txn) =>
            {
                await CrudProvider.DeleteAsync<TModel>(cn, id, txn, User);
                return default;
            }, txnAction);
        }

        private async Task<TIdentity> ExecuteInnerAsync<TModel>(
            Func<IDbConnection, IDbTransaction, Task<TIdentity>> crudAction,
            Func<IDbConnection, IDbTransaction, Task> txnAction = null)
        {
            using (var cn = GetConnection())
            {
                if (cn.State == ConnectionState.Closed) cn.Open();
                using (var txn = cn.BeginTransaction())
                {
                    try
                    {
                        var result = await crudAction.Invoke(cn, txn);
                        if (txnAction != null) await txnAction.Invoke(cn, txn);
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
        }

        #region Try methods
        public async Task<bool> TryUpdateUserAsync(Func<Task> onSuccess = null, Func<Exception, Task> onException = null)
        {
            try
            {
                await UpdateUserAsync();
                if (onSuccess != null) await onSuccess.Invoke();
                return true;
            }
            catch (Exception exc)
            {
                if (onException != null) await onException.Invoke(exc);
                return false;
            }
        }

        public async Task<bool> TrySaveAsync<TModel>(
            TModel model, ChangeTracker<TModel> changeTracker = null,
            Func<TIdentity, Task> onSuccess = null, Func<Exception, Task> onException = null)
        {
            try
            {
                var result = await SaveAsync(model, changeTracker);
                if (onSuccess != null) await onSuccess.Invoke(result);
                return true;
            }
            catch (Exception exc)
            {
                if (onException != null) await onException.Invoke(exc);
                return false;
            }            
        }

        public async Task<bool> TryMergeAsync<TModel>(
            TModel model, ChangeTracker<TModel> changeTracker = null,
            Func<TIdentity, Task> onSuccess = null, Func<Exception, Task> onException = null)
        {
            try
            {
                var result = await MergeAsync(model, changeTracker);
                if (onSuccess != null) await onSuccess.Invoke(result);
                return true;
            }
            catch (Exception exc)
            {
                if (onException != null) await onException.Invoke(exc);
                return false;
            }
        }

        public async Task<bool> TryInsertAsync<TModel>(TModel model, Func<TIdentity, Task> onSuccess = null, Func<Exception, Task> onException = null)
        {
            try
            {
                var result = await InsertAsync(model);
                if (onSuccess != null) await onSuccess.Invoke(result);
                return true;
            }
            catch (Exception exc)
            {
                if (onException != null) await onException.Invoke(exc);
                return false;
            }
        }

        public async Task<bool> TryDeleteAsync<TModel>(TIdentity id, Func<Task> onSuccess = null, Func<Exception, Task> onException = null)
        {
            try
            {
                await DeleteAsync<TModel>(id);
                if (onSuccess != null) await onSuccess.Invoke();
                return true;
            }
            catch (Exception exc)
            {
                if (onException != null) await onException.Invoke(exc);
                return false;
            }
        }

        public async Task<bool> TryUpdateAsync<TModel>(
            TModel model, ChangeTracker<TModel> changeTracker = null,
            Func<Task> onSuccess = null, Func<Exception, Task> onException = null)
        {            
            try
            {
                await UpdateAsync(model, changeTracker);
                if (onSuccess != null) await onSuccess.Invoke();
                return true;
            }
            catch (Exception exc)
            {
                if (onException != null) await onException.Invoke(exc);
                return false;
            }
        }
        #endregion
    }
}
