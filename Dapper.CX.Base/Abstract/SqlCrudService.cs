using AO.DbSchema.Enums;
using Dapper.CX.Classes;
using System;
using System.Data;
using System.Threading.Tasks;

namespace Dapper.CX.Abstract
{
    public abstract class SqlCrudService<TIdentity>
    {        
        private readonly SqlCrudProvider<TIdentity> _crudProvider;

        protected readonly string _connectionString;

        public SqlCrudService(string connectionString, SqlCrudProvider<TIdentity> crudProvider)
        {
            _connectionString = connectionString;
            _crudProvider = crudProvider;
        }

        public abstract IDbConnection GetConnection();

        public async Task<TModel> GetAsync<TModel>(TIdentity id, IDbTransaction txn = null)
        {
            using (var cn = GetConnection())
            {
                return await _crudProvider.GetAsync<TModel>(cn, id, txn);
            }
        }

        public async Task<TModel> GetWhereAsync<TModel>(object criteria, IDbTransaction txn = null)
        {
            using (var cn = GetConnection())
            {
                return await _crudProvider.GetWhereAsync<TModel>(cn, criteria, txn);
            }
        }

        public async Task<bool> ExistsAsync<TModel>(TIdentity id, IDbTransaction txn = null)
        {
            using (var cn = GetConnection())
            {
                return await _crudProvider.ExistsAsync<TModel>(cn, id, null);
            }
        }

        public async Task<bool> ExistsWhereAsync<TModel>(object criteria, IDbTransaction txn = null)
        {
            using (var cn = GetConnection())
            {
                return await _crudProvider.ExistsWhereAsync<TModel>(cn, criteria, txn);
            }
        }

        public async Task<TIdentity> SaveAsync<TModel>(TModel model, ChangeTracker<TModel> changeTracker = null, Action<TModel, SaveAction> onSave = null, IDbTransaction txn = null)
        {
            using (var cn = GetConnection())
            {
                return await _crudProvider.SaveAsync(cn, model, changeTracker, onSave, txn);
            }
        }        

        public async Task<TIdentity> MergeAsync<TModel>(TModel model, ChangeTracker<TModel> changeTracker = null, Action<TModel, SaveAction> onSave = null, IDbTransaction txn = null)
        {
            using (var cn = GetConnection())
            {
                return await _crudProvider.MergeAsync(cn, model, changeTracker, onSave, null);
            }
        }        

        public async Task<TIdentity> InsertAsync<TModel>(TModel model, Action<TModel, SaveAction> onSave = null, IDbTransaction txn = null)
        {
            using (var cn = GetConnection())
            {
                return await _crudProvider.InsertAsync(cn, model, onSave, txn: txn);
            }
        }        

        public async Task UpdateAsync<TModel>(TModel model, ChangeTracker<TModel> changeTracker = null, Action<TModel, SaveAction> onSave = null, IDbTransaction txn = null)
        {
            using (var cn = GetConnection())
            {
                await _crudProvider.UpdateAsync(cn, model, changeTracker, onSave, txn);
            }
        }        

        public async Task DeleteAsync<TModel>(TIdentity id, IDbTransaction txn = null)
        {
            using (var cn = GetConnection())
            {
                await _crudProvider.DeleteAsync<TModel>(cn, id, txn);
            }
        }

        public async Task<Result> TrySaveAsync<TModel>(TModel model, ChangeTracker<TModel> changeTracker = null, Action<TModel, SaveAction> onSave = null, IDbTransaction txn = null)
        {
            var result = new Result();

            try
            {
                result.Id = await SaveAsync(model, changeTracker, onSave, txn);
                result.IsSuccessful = true;
            }
            catch (Exception exc)
            {
                result.Exception = exc;
            }

            return result;
        }

        public async Task<Result> TryMergeAsync<TModel>(TModel model, ChangeTracker<TModel> changeTracker = null, Action<TModel, SaveAction> onSave = null, IDbTransaction txn = null)
        {
            var result = new Result();

            try
            {
                result.Id = await MergeAsync(model, changeTracker, onSave, txn);
                result.IsSuccessful = true;
            }
            catch (Exception exc)
            {
                result.Exception = exc;
            }

            return result;
        }

        public async Task<Result> TryInsertAsync<TModel>(TModel model, Action<TModel, SaveAction> onSave = null, IDbTransaction txn = null)
        {
            var result = new Result();

            try
            {
                result.Id = await InsertAsync(model, onSave, txn);
                result.IsSuccessful = true;
            }
            catch (Exception exc)
            {
                result.Exception = exc;
            }

            return result;
        }

        public async Task<Result> TryDeleteAsync<TModel>(TIdentity id, IDbTransaction txn = null)
        {
            var result = new Result();

            try
            {
                await DeleteAsync<TModel>(id, txn);
                result.IsSuccessful = true;
            }
            catch (Exception exc)
            {
                result.Exception = exc;
            }

            return result;
        }

        public async Task<Result> TryUpdateAsync<TModel>(TModel model, ChangeTracker<TModel> changeTracker = null, Action<TModel, SaveAction> onSave = null, IDbTransaction txn = null)
        {
            var result = new Result();

            try
            {
                await UpdateAsync(model, changeTracker, onSave, txn);
                result.IsSuccessful = true;
            }
            catch (Exception exc)
            {
                result.Exception = exc;
            }

            return result;
        }

        public class Result
        {
            public bool IsSuccessful { get; set; }
            public TIdentity Id { get; set; }
            public Exception Exception { get; set; }
        }
    }
}
