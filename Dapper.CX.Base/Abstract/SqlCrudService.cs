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

        public async Task DeleteAsync(TIdentity id)
        {
            using (var cn = GetConnection(_connectionString))
            {
                await _crudProvider.DeleteAsync<TIdentity>(cn, id);
            }
        }
    }
}
