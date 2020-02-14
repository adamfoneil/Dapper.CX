using AO.DbSchema.Enums;
using Dapper.CX.Classes;
using System;
using System.Data;
using System.Threading.Tasks;

namespace Dapper.CX.Abstract
{
    public abstract class SqlCrudService<TIdentity>
    {
        private readonly string _connectionString;
        private readonly SqlCrudProvider<TIdentity> _crudProvider;

        public SqlCrudService(string connectionString, SqlCrudProvider<TIdentity> crudProvider)
        {
            _connectionString = connectionString;
            _crudProvider = crudProvider;
        }

        protected abstract IDbConnection GetConnection(string connectionString);

        public async Task<TModel> GetAsync<TModel>(TIdentity id)
        {
            using (var cn = GetConnection(_connectionString))
            {
                return await _crudProvider.GetAsync<TModel>(cn, id);
            }
        }

        public async Task<TModel> GetWhereAsync<TModel>(object criteria)
        {
            using (var cn = GetConnection(_connectionString))
            {
                return await _crudProvider.GetWhereAsync<TModel>(cn, criteria);
            }
        }

        public async Task<bool> ExistsAsync<TModel>(TIdentity id)
        {
            using (var cn = GetConnection(_connectionString))
            {
                return await _crudProvider.ExistsAsync<TModel>(cn, id);
            }
        }

        public async Task<bool> ExistsWhereAsync<TModel>(object criteria)
        {
            using (var cn = GetConnection(_connectionString))
            {
                return await _crudProvider.ExistsWhereAsync<TModel>(cn, criteria);
            }
        }

        public async Task<TIdentity> SaveAsync<TModel>(TModel model, ChangeTracker<TModel> changeTracker = null, Action<TModel, SaveAction> onSave = null)
        {
            using (var cn = GetConnection(_connectionString))
            {
                return await _crudProvider.SaveAsync(cn, model, changeTracker, onSave);
            }
        }

        public async Task<TIdentity> MergeAsync<TModel>(TModel model, ChangeTracker<TModel> changeTracker = null, Action<TModel, SaveAction> onSave = null)
        {
            using (var cn = GetConnection(_connectionString))
            {
                return await _crudProvider.MergeAsync(cn, model, changeTracker, onSave);
            }
        }

        public async Task<TIdentity> InsertAsync<TModel>(TModel model, Action<TModel, SaveAction> onSave = null)
        {
            using (var cn = GetConnection(_connectionString))
            {
                return await _crudProvider.InsertAsync(cn, model, onSave);
            }
        }

        public async Task UpdateAsync<TModel>(TModel model, ChangeTracker<TModel> changeTracker = null, Action<TModel, SaveAction> onSave = null)
        {
            using (var cn = GetConnection(_connectionString))
            {
                await _crudProvider.UpdateAsync(cn, model, changeTracker, onSave);
            }
        }

        public async Task DeleteAsync(TIdentity id)
        {
            using (var cn = GetConnection(_connectionString))
            {
                await _crudProvider.DeleteAsync<TIdentity>(cn, id);
            }
        }
    }
}
