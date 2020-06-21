using AO.Models.Interfaces;
using Dapper.CX.Classes;
using System;
using System.Data;
using System.Threading.Tasks;

namespace Dapper.CX.Abstract
{
    public abstract class SqlCrudService<TIdentity>
    {
        private readonly SqlCrudProvider<TIdentity> _crudProvider;

        public SqlCrudService(SqlCrudProvider<TIdentity> crudProvider)
        {            
            _crudProvider = crudProvider;            
        }

        public abstract IDbConnection GetConnection();

        public async Task<TModel> GetAsync<TModel>(TIdentity id, IUserBase user = null)
        {
            using (var cn = GetConnection())
            {
                return await _crudProvider.GetAsync<TModel>(cn, id, user: user);
            }
        }

        public async Task<TModel> GetWhereAsync<TModel>(object criteria, IUserBase user = null)
        {
            using (var cn = GetConnection())
            {
                return await _crudProvider.GetWhereAsync<TModel>(cn, criteria, user: user);
            }
        }

        public async Task<bool> ExistsAsync<TModel>(TIdentity id)
        {
            using (var cn = GetConnection())
            {
                return await _crudProvider.ExistsAsync<TModel>(cn, id);
            }
        }

        public async Task<bool> ExistsWhereAsync<TModel>(object criteria)
        {
            using (var cn = GetConnection())
            {
                return await _crudProvider.ExistsWhereAsync<TModel>(cn, criteria);
            }
        }

        public async Task<TIdentity> SaveAsync<TModel>(TModel model, string[] columnNames)
        {
            using (var cn = GetConnection())
            {
                return await _crudProvider.SaveAsync(cn, model, columnNames);
            }
        }

        public async Task<TIdentity> SaveAsync<TModel>(
            TModel model, ChangeTracker<TModel> changeTracker = null, IUserBase user = null, 
            Func<IDbConnection, IDbTransaction, Task> txnAction = null)
        {            
            return await ExecuteInnerAsync<TModel>((cn, txn) => _crudProvider.SaveAsync(cn, model, changeTracker, txn, user), txnAction);            
        }

        public async Task<TIdentity> MergeAsync<TModel>(
            TModel model, ChangeTracker<TModel> changeTracker = null, IUserBase user = null, 
            Func<IDbConnection, IDbTransaction, Task> txnAction = null)
        {
            return await ExecuteInnerAsync<TModel>((cn, txn) => _crudProvider.MergeAsync(cn, model, changeTracker, txn, user), txnAction);
        }

        public async Task<TIdentity> InsertAsync<TModel>(
            TModel model, IUserBase user = null,
            Func<IDbConnection, IDbTransaction, Task> txnAction = null)
        {
            return await ExecuteInnerAsync<TModel>((cn, txn) => _crudProvider.InsertAsync(cn, model, getIdentity: true, txn, user), txnAction);
        }

        public async Task UpdateAsync<TModel>(
            TModel model, ChangeTracker<TModel> changeTracker = null, IUserBase user = null,
            Func<IDbConnection, IDbTransaction, Task> txnAction = null)
        {
            await ExecuteInnerAsync<TModel>(async (cn, txn) => 
            { 
                await _crudProvider.UpdateAsync(cn, model, changeTracker, txn, user); 
                return default; 
            }, txnAction);
        }

        public async Task DeleteAsync<TModel>(
            TModel model, IUserBase user = null,
            Func<IDbConnection, IDbTransaction, Task> txnAction = null)
        {
            await ExecuteInnerAsync<TModel>(async (cn, txn) =>
            {
                await _crudProvider.DeleteAsync(cn, model, txn, user);
                return default;
            }, txnAction);
        }

        public async Task DeleteAsync<TModel>(
            TIdentity id, IUserBase user = null,
            Func<IDbConnection, IDbTransaction, Task> txnAction = null)
        {
            await ExecuteInnerAsync<TModel>(async (cn, txn) => 
            {
                await _crudProvider.DeleteAsync<TModel>(cn, id, txn, user);
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
        public async Task<Result> TrySaveAsync<TModel>(TModel model, ChangeTracker<TModel> changeTracker = null, IUserBase user = null)
        {
            var result = new Result();

            try
            {
                result.Id = await SaveAsync(model, changeTracker, user: user);
                result.IsSuccessful = true;
            }
            catch (Exception exc)
            {
                result.Exception = exc;
            }

            return result;
        }

        public async Task<Result> TryMergeAsync<TModel>(TModel model, ChangeTracker<TModel> changeTracker = null, IUserBase user = null)
        {
            var result = new Result();

            try
            {
                result.Id = await MergeAsync(model, changeTracker, user: user);
                result.IsSuccessful = true;
            }
            catch (Exception exc)
            {
                result.Exception = exc;
            }

            return result;
        }

        public async Task<Result> TryInsertAsync<TModel>(TModel model, IUserBase user = null)
        {
            var result = new Result();

            try
            {
                result.Id = await InsertAsync(model, user: user);
                result.IsSuccessful = true;
            }
            catch (Exception exc)
            {
                result.Exception = exc;
            }

            return result;
        }

        public async Task<Result> TryDeleteAsync<TModel>(TIdentity id, IUserBase user = null)
        {
            var result = new Result();

            try
            {
                await DeleteAsync<TModel>(id, user: user);
                result.IsSuccessful = true;
            }
            catch (Exception exc)
            {
                result.Exception = exc;
            }

            return result;
        }

        public async Task<Result> TryUpdateAsync<TModel>(TModel model, ChangeTracker<TModel> changeTracker = null, IUserBase user = null)
        {
            var result = new Result();

            try
            {
                await UpdateAsync(model, changeTracker, user: user);
                result.IsSuccessful = true;
            }
            catch (Exception exc)
            {
                result.Exception = exc;
            }

            return result;
        }
        #endregion

        public class Result
        {
            public bool IsSuccessful { get; set; }
            public TIdentity Id { get; set; }
            public Exception Exception { get; set; }
        }
    }
}
