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

        public SqlCrudService(SqlCrudProvider<TIdentity> crudProvider)
        {
            _crudProvider = crudProvider;
        }

        protected abstract IDbConnection GetConnection();

        public async Task<TIdentity> SaveAsync<TModel>(TModel model, ChangeTracker<TModel> changeTracker = null, Action<TModel, SaveAction> onSave = null)
        {
            using (var cn = GetConnection())
            {
                return await _crudProvider.SaveAsync(cn, model, changeTracker, onSave);
            }
        }

        public async Task<TIdentity> MergeAsync<TModel>(TModel model, ChangeTracker<TModel> changeTracker = null, Action<TModel, SaveAction> onSave = null)
        {
            using (var cn = GetConnection())
            {
                return await _crudProvider.MergeAsync(cn, model, changeTracker, onSave);
            }
        }

        public async Task DeleteAsync(TIdentity id)
        {
            using (var cn = GetConnection())
            {
                await _crudProvider.DeleteAsync<TIdentity>(cn, id);
            }
        }
    }
}
