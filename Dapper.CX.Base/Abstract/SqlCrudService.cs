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

        public SqlCrudService(SqlCrudProvider<TIdentity> crudProvider, string userName)
        {            
            CrudProvider = crudProvider;
            UserName = userName;

            if (!string.IsNullOrEmpty(userName))
            {
                using (var cn = GetConnection())
                {
                    CurrentUser = QueryUser(cn, userName);
                }
            }
        }

        public abstract IDbConnection GetConnection();

        public string UserName { get; }
        public TUser CurrentUser { get; }

        protected abstract TUser QueryUser(IDbConnection connection, string userName);

        public async Task<TModel> GetAsync<TModel>(TIdentity id)
        {
            using (var cn = GetConnection())
            {
                return await CrudProvider.GetAsync<TModel>(cn, id, user: CurrentUser);
            }
        }

        public async Task<TModel> GetWhereAsync<TModel>(object criteria)
        {
            using (var cn = GetConnection())
            {
                return await CrudProvider.GetWhereAsync<TModel>(cn, criteria, user: CurrentUser);
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
            return await ExecuteInnerAsync<TModel>((cn, txn) => CrudProvider.SaveAsync(cn, model, changeTracker, txn, CurrentUser), txnAction);            
        }

        public async Task<TIdentity> MergeAsync<TModel>(
            TModel model, ChangeTracker<TModel> changeTracker = null, 
            Func<IDbConnection, IDbTransaction, Task> txnAction = null)
        {
            return await ExecuteInnerAsync<TModel>((cn, txn) => CrudProvider.MergeAsync(cn, model, changeTracker, txn, CurrentUser), txnAction);
        }

        public async Task<TIdentity> InsertAsync<TModel>(
            TModel model, Func<IDbConnection, IDbTransaction, Task> txnAction = null)
        {
            return await ExecuteInnerAsync<TModel>((cn, txn) => CrudProvider.InsertAsync(cn, model, getIdentity: true, txn, CurrentUser), txnAction);
        }

        public async Task UpdateAsync<TModel>(
            TModel model, ChangeTracker<TModel> changeTracker = null,
            Func<IDbConnection, IDbTransaction, Task> txnAction = null)
        {
            await ExecuteInnerAsync<TModel>(async (cn, txn) => 
            { 
                await CrudProvider.UpdateAsync(cn, model, changeTracker, txn, CurrentUser); 
                return default; 
            }, txnAction);
        }

        public async Task DeleteAsync<TModel>(
            TModel model, Func<IDbConnection, IDbTransaction, Task> txnAction = null)
        {
            await ExecuteInnerAsync<TModel>(async (cn, txn) =>
            {
                await CrudProvider.DeleteAsync(cn, model, txn, CurrentUser);
                return default;
            }, txnAction);
        }

        public async Task DeleteAsync<TModel>(
            TIdentity id, Func<IDbConnection, IDbTransaction, Task> txnAction = null)
        {
            await ExecuteInnerAsync<TModel>(async (cn, txn) => 
            {
                await CrudProvider.DeleteAsync<TModel>(cn, id, txn, CurrentUser);
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
        public async Task<Result> TrySaveAsync<TModel>(TModel model, ChangeTracker<TModel> changeTracker = null)
        {
            var result = new Result();

            try
            {
                result.Id = await SaveAsync(model, changeTracker);
                result.IsSuccessful = true;
            }
            catch (Exception exc)
            {
                result.Exception = exc;
            }

            return result;
        }

        public async Task<Result> TryMergeAsync<TModel>(TModel model, ChangeTracker<TModel> changeTracker = null)
        {
            var result = new Result();

            try
            {
                result.Id = await MergeAsync(model, changeTracker);
                result.IsSuccessful = true;
            }
            catch (Exception exc)
            {
                result.Exception = exc;
            }

            return result;
        }

        public async Task<Result> TryInsertAsync<TModel>(TModel model)
        {
            var result = new Result();

            try
            {
                result.Id = await InsertAsync(model);
                result.IsSuccessful = true;
            }
            catch (Exception exc)
            {
                result.Exception = exc;
            }

            return result;
        }

        public async Task<Result> TryDeleteAsync<TModel>(TIdentity id)
        {
            var result = new Result();

            try
            {
                await DeleteAsync<TModel>(id);
                result.IsSuccessful = true;
            }
            catch (Exception exc)
            {
                result.Exception = exc;
            }

            return result;
        }

        public async Task<Result> TryUpdateAsync<TModel>(TModel model, ChangeTracker<TModel> changeTracker = null)
        {
            var result = new Result();

            try
            {
                await UpdateAsync(model, changeTracker);
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
