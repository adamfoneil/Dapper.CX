using Dapper.CX.Base.Classes;
using Dapper.CX.Base.Enums;
using Dapper.CX.Base.Exceptions;
using Dapper.CX.Base.Extensions;
using System;
using System.Data;
using System.Threading.Tasks;

namespace Dapper.CX.Base.Abstract
{
    public abstract class SqlCrudProvider<TIdentity>
    {
        protected abstract string SelectIdentityCommand { get; }
        protected abstract char StartDelimiter { get; }
        protected abstract char EndDelimiter { get; }

        /// <summary>
        /// Types supported by this handler when mapping to an object.
        /// </summary>
        protected abstract Type[] SupportedTypes { get; }

        protected abstract TIdentity ConvertIdentity(object identity);

        public SaveAction SaveAction { get; private set; }

        public TIdentity GetIdentity<TModel>(TModel model)
        {
            var idProperty = typeof(TModel).GetIdentityProperty();
            object idValue = idProperty.GetValue(model);
            return ConvertIdentity(idValue);
        }

        public bool IsNew<TModel>(TModel model)
        {
            return GetIdentity(model).Equals(default);
        }

        public async Task<TModel> GetAsync<TModel>(IDbConnection connection, TIdentity identity, CrudOptions<TModel> options = null)
        {

        }

        public async Task<TModel> GetWhereAsync<TModel>(IDbConnection connection, object criteria, CrudOptions<TModel> options = null)
        {

        }

        public async Task<TIdentity> SaveAsync<TModel>(IDbConnection connection, TModel model, CrudOptions<TModel> options = null)
        {
            if (IsNew(model))
            {
                return await InsertAsync(connection, model, options);
            }
            else
            {
                await UpdateAsync(connection, model, options);
                return GetIdentity(model);
            }
        }

        public async Task<TIdentity> InsertAsync<TModel>(IDbConnection connection, TModel model, CrudOptions<TModel> options = null)
        {
            try
            {
                // save here
                SaveAction = SaveAction.Insert;
            }
            catch (Exception exc)
            {
                
            }            
        }

        public async Task UpdateAsync<TModel>(IDbConnection connection, TModel model, CrudOptions<TModel> options = null)
        {
            try
            {

                SaveAction = SaveAction.Update;
            }
            catch (Exception exc)
            {

                throw;
            }
        }

        public async Task DeleteAsync(IDbConnection connection, TIdentity id)
        {
            throw new NotImplementedException();
        }

        #region SQL statements
        public string GetQuerySingleStatement<TModel>(TModel model)
        {
            throw new NotImplementedException();
        }

        public string GetInsertStatement<TModel>(TModel model)
        {
            throw new NotImplementedException();
        }

        public string GetUpdateStatement<TModel>(TModel model, ChangeTracker<TModel> changeTracker = null)
        {
            throw new NotImplementedException();
        }

        public string GetDeleteStatement<TModel>()
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
