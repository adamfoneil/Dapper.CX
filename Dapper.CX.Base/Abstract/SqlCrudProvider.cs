using Dapper.CX.Classes;
using Dapper.CX.Enums;
using Dapper.CX.Exceptions;
using Dapper.CX.Extensions;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Dapper.CX.Abstract
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

        public async Task<TModel> GetAsync<TModel>(IDbConnection connection, TIdentity identity)
        {
            return await connection.QuerySingleOrDefaultAsync<TModel>(GetQuerySingleStatement(typeof(TModel)), new { id = identity });
        }

        public async Task<TModel> GetWhereAsync<TModel>(IDbConnection connection, object criteria)
        {
            return await connection.QuerySingleOrDefaultAsync<TModel>(GetQuerySingleWhereStatement(typeof(TModel), criteria), criteria);
        }

        public async Task<TIdentity> SaveAsync<TModel>(IDbConnection connection, TModel model, ChangeTracker<TModel> changeTracker = null, Action<TModel, SaveAction> onSave = null)
        {
            if (IsNew(model))
            {
                return await InsertAsync(connection, model, onSave);
            }
            else
            {                
                await UpdateAsync(connection, model, changeTracker, onSave);
                return GetIdentity(model);
            }
        }

        public async Task<TIdentity> InsertAsync<TModel>(IDbConnection connection, TModel model, Action<TModel, SaveAction> onSave = null)
        {
            onSave?.Invoke(model, SaveAction.Insert);
            var cmd = new CommandDefinition(GetInsertStatement(typeof(TModel)), model);

            try
            {                
                return await connection.QuerySingleOrDefaultAsync<TIdentity>(cmd);
            }
            catch (Exception exc)
            {
                throw new CrudException(cmd, exc);
            }            
        }

        public async Task UpdateAsync<TModel>(IDbConnection connection, TModel model, ChangeTracker<TModel> changeTracker = null, Action<TModel, SaveAction> onSave = null)
        {
            onSave?.Invoke(model, SaveAction.Update);
            var cmd = new CommandDefinition(GetUpdateStatement(model, changeTracker), model);

            try
            {                               
                await connection.ExecuteAsync(cmd);
            }
            catch (Exception exc)
            {
                throw new CrudException(cmd, exc);
            }
        }

        public async Task DeleteAsync<TModel>(IDbConnection connection, TIdentity id)
        {
            var cmd = new CommandDefinition(GetDeleteStatement(typeof(TModel)), new { id });

            try
            {
                await connection.ExecuteAsync(cmd);
            }
            catch (Exception exc)
            {
                throw new CrudException(cmd, exc);
            }
        }

        #region SQL statements
        public string GetQuerySingleStatement(Type modelType)
        {
            return $"SELECT * FROM {ApplyDelimiter(modelType.GetTableName())} WHERE {ApplyDelimiter(modelType.GetIdentityName())}=@id";
        }

        public string GetQuerySingleWhereStatement(Type modelType, object criteria)
        {
            var properties = criteria.GetType().GetProperties().Select(pi => pi.Name);
            return $"SELECT * FROM {ApplyDelimiter(modelType.GetTableName())} WHERE {string.Join(" AND ", properties.Select(name => ApplyDelimiter(name) + "=@" + name))}";
        }

        public string GetInsertStatement(Type modelType)
        {
            var columns = GetMappedProperties(modelType, SaveAction.Insert).Select(pi => pi.GetColumnName());

            return
                $@"INSERT INTO {ApplyDelimiter(modelType.GetTableName())} (
                    {string.Join(", ", columns.Select(col => ApplyDelimiter(col)))}
                ) VALUES (
                    {string.Join(", ", columns.Select(col => "@" + col))}
                ); " + SelectIdentityCommand;
        }

        public string GetUpdateStatement<TModel>(TModel model, ChangeTracker<TModel> changeTracker = null)
        {
            var columns = 
                changeTracker?.GetModifiedColumns(SaveAction.Update) ?? 
                GetMappedProperties(typeof(TModel), SaveAction.Update).Select(pi => pi.GetColumnName());

            var type = typeof(TModel);
            string identityCol = type.GetIdentityName();

            return 
                $@"UPDATE {ApplyDelimiter(type.GetTableName())} SET 
                    {string.Join(", ", columns.Select(col => $"{ApplyDelimiter(col)}=@{col}"))} 
                WHERE 
                    {ApplyDelimiter(identityCol)}=@{identityCol}";
        }

        private PropertyInfo[] GetMappedProperties(Type modelType, SaveAction saveAction)
        {
            bool isMapped(PropertyInfo pi)
            {
                if (pi.IsIdentity()) return false;
                if (!SupportedTypes.Contains(pi.PropertyType)) return false;
                if (!pi.AllowSaveAction(saveAction)) return false;

                var attr = pi.GetCustomAttribute<NotMappedAttribute>();
                if (attr != null) return false;

                return true;
            };

            return modelType.GetProperties().Where(pi => isMapped(pi)).ToArray();
        }

        public string GetDeleteStatement(Type modelType)
        {                        
            return $@"DELETE {ApplyDelimiter(modelType.GetTableName())} WHERE {ApplyDelimiter(modelType.GetIdentityName())}=@id";
        }

        protected string ApplyDelimiter(string name)
        {
            return string.Join(".", name
                .Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(namePart => $"{StartDelimiter}{namePart}{EndDelimiter}"));
        }

        #endregion
    }
}
