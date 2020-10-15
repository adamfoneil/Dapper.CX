using AO.Models.Attributes;
using AO.Models.Enums;
using AO.Models.Interfaces;
using AO.Models.Static;
using Dapper.CX.Classes;
using Dapper.CX.Exceptions;
using Dapper.CX.Extensions;
using Dapper.CX.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Dapper.CX.Abstract
{
    public abstract partial class SqlCrudProvider<TIdentity> : ISqlCrudProvider<TIdentity>
    {
        private readonly Func<object, TIdentity> _convertIdentity;

        public SqlCrudProvider(Func<object, TIdentity> convertIdentity)
        {
            _convertIdentity = convertIdentity;
        }

        protected abstract string SelectIdentityCommand { get; }
        protected abstract char StartDelimiter { get; }
        protected abstract char EndDelimiter { get; }

        /// <summary>
        /// Types supported by this handler when mapping to an object.
        /// </summary>
        protected abstract Type[] SupportedTypes { get; }

        public TIdentity GetIdentity<TModel>(TModel model)
        {
            var idProperty = typeof(TModel).GetIdentityProperty();
            object idValue = idProperty.GetValue(model);
            return _convertIdentity(idValue);
        }

        public bool IsNew<TModel>(TModel model)
        {
            return GetIdentity(model).Equals(default(TIdentity));
        }

        public async Task<TModel> GetAsync<TModel>(IDbConnection connection, TIdentity identity, IDbTransaction txn = null, IUserBase user = null)
        {
            var result = await connection.QuerySingleOrDefaultAsync<TModel>(GetQuerySingleStatement(typeof(TModel)), new { id = identity }, txn);

            if (user != null && result != null)
            {
                await VerifyGetPermission(connection, identity, txn, user, result);
                await VerifyTenantIsolation(connection, user, result, txn);
            }

            await OnGetRelatedAsync(connection, result, txn);

            return result;
        }

        public async Task<TModel> GetWhereAsync<TModel>(IDbConnection connection, object criteria, IDbTransaction txn = null, IUserBase user = null)
        {
            var result = await connection.QuerySingleOrDefaultAsync<TModel>(GetQuerySingleWhereStatement(typeof(TModel), criteria), criteria, txn);

            if (user != null && result != null)
            {
                await VerifyGetPermission(connection, GetIdentity(result), txn, user, result);
                await VerifyTenantIsolation(connection, user, result, txn);
            }

            await OnGetRelatedAsync(connection, result, txn);

            return result;
        }

        private static async Task OnGetRelatedAsync<TModel>(IDbConnection connection, TModel result, IDbTransaction txn = null)
        {
            if (result == null) return;

            var getRelated = result as IGetRelated;
            if (getRelated != null) await getRelated.GetRelatedAsync(connection, txn);
        }

        public async Task<TIdentity> SaveAsync<TModel>(IDbConnection connection, TModel model, ChangeTracker<TModel> changeTracker = null, IDbTransaction txn = null, IUserBase user = null)
        {
            if (IsNew(model))
            {
                return await InsertAsync(connection, model, getIdentity: true, txn, user);
            }
            else
            {
                await UpdateAsync(connection, model, changeTracker, txn, user);
                return GetIdentity(model);
            }
        }

        private async Task<ChangeTracker<TModel>> InitChangeTrackingAsync<TModel>(IDbConnection connection, IDbTransaction txn, TModel model, IUserBase user, ChangeTracker<TModel> changeTracker)
        {
            if (changeTracker != null) return changeTracker;

            if (typeof(TModel).HasAttribute(out TrackChangesAttribute attr))
            {
                var identity = GetIdentity(model);
                var existing = await GetAsync<TModel>(connection, identity, txn);
                if (existing == null) return null;

                var result = new LoggedChangeTracker<TModel, TIdentity>(this, user, existing);
                result.Instance = model;
                
                foreach (var ignore in attr.GetIgnoreProperties())
                {
                    if (result.ContainsKey(ignore)) result.Remove(ignore);
                }

                return result;
            }

            return null;
        }

        public async Task<TIdentity> MergeAsync<TModel>(IDbConnection connection, TModel model, IEnumerable<string> keyProperties, ChangeTracker<TModel> changeTracker = null, IDbTransaction txn = null, IUserBase user = null)
        {
            if (IsNew(model))
            {
                var existing = await GetByPropertiesAsync(connection, model, keyProperties, txn);
                if (existing != null) SetIdentity(model, GetIdentity(existing));
            }

            return await SaveAsync(connection, model, changeTracker, txn, user);
        }

        public async Task<TIdentity> MergeAsync<TModel>(IDbConnection connection, TModel model, ChangeTracker<TModel> changeTracker = null, IDbTransaction txn = null, IUserBase user = null)
        {
            var props = typeof(TModel).GetProperties().Where(pi => pi.HasAttribute<KeyAttribute>()).Select(pi => pi.GetColumnName());
            if (!props.Any()) throw new Exception($"No primary key properties found on {typeof(TModel).Name}");

            return await MergeAsync(connection, model, props, changeTracker, txn, user);
        }

        private void SetIdentity<TModel>(TModel model, TIdentity identity)
        {
            if (IsNew(model))
            {
                var identityProp = typeof(TModel).GetIdentityProperty();
                identityProp.SetValue(model, identity);
            }
            else
            {
                throw new InvalidOperationException("Can't set a record's identity more than once.");
            }
        }

        private async Task<TModel> GetByPropertiesAsync<TModel>(IDbConnection connection, TModel model, IEnumerable<string> properties, IDbTransaction txn = null)
        {
            string sql = GetQuerySingleWhereStatement(typeof(TModel), properties);
            return await connection.QuerySingleOrDefaultAsync<TModel>(sql, model, txn);
        }

        public async Task<TIdentity> InsertAsync<TModel>(IDbConnection connection, TModel model, bool getIdentity = true, IDbTransaction txn = null, IUserBase user = null)
        {
            await ValidateInternal(connection, model, txn);

            AuditRow(model, SaveAction.Insert, user);

            await VerifyTenantIsolation(connection, user, model, txn);

            var cmd = new CommandDefinition(GetInsertStatement(typeof(TModel), getIdentity: getIdentity), model, txn);

            Debug.Print(cmd.CommandText);

            try
            {
                TIdentity result = await connection.QuerySingleOrDefaultAsync<TIdentity>(cmd);
                if (getIdentity) SetIdentity(model, result);

                await ExecuteSaveTrigger(connection, SaveAction.Insert, model, txn, user);

                return result;
            }
            catch (Exception exc)
            {
                throw new CrudException(cmd, exc);
            }
        }

        private async static Task ExecuteSaveTrigger<TModel>(IDbConnection connection, SaveAction saveAction, TModel model, IDbTransaction txn = null, IUserBase user = null)
        {
            try
            {
                var trigger = model as ITrigger;
                if (trigger != null)
                {
                    await trigger.RowSavedAsync(connection, saveAction, txn, user);
                }
            }
            catch (Exception exc)
            {
                throw new TriggerException(exc, TriggerAction.Save);
            }
        }

        public async Task UpdateAsync<TModel>(IDbConnection connection, TModel model, ChangeTracker<TModel> changeTracker = null, IDbTransaction txn = null, IUserBase user = null)
        {
            changeTracker = await InitChangeTrackingAsync(connection, txn, model, user, changeTracker);

            await ValidateInternal(connection, model, txn);

            AuditRow(model, SaveAction.Update, user);

            await VerifyTenantIsolation(connection, user, model, txn);

            var cmd = new CommandDefinition(GetUpdateStatement(changeTracker), model, txn);

            Debug.Print(cmd.CommandText);

            try
            {
                await connection.ExecuteAsync(cmd);

                await ExecuteSaveTrigger(connection, SaveAction.Update, model, txn, user);

                var saveable = changeTracker as IDbSaveable;
                if (saveable != null)
                {
                    try
                    {
                        await saveable.SaveAsync(connection, txn);
                    }
                    catch (Exception exc)
                    {
                        throw new ChangeTrackerSaveException(exc);
                    }
                }
            }
            catch (Exception exc)
            {
                throw new CrudException(cmd, exc);
            }
        }

        public async Task DeleteAsync<TModel>(IDbConnection connection, TModel model, IDbTransaction txn = null, IUserBase user = null)
        {
            if (model == null) throw new ArgumentException(nameof(model));

            var id = GetIdentity(model);

            await AllowDeleteAsync(connection, id, txn, user, model);

            await DeleteInnerAsync<TModel>(connection, txn, id);

            await ExecuteDeleteTrigger(connection, model, txn, user);
        }

        public async Task DeleteAsync<TModel>(IDbConnection connection, TIdentity id, IDbTransaction txn = null, IUserBase user = null)
        {
            var model = await AllowDeleteAsync<TModel>(connection, id, txn, user);

            await DeleteInnerAsync<TModel>(connection, txn, id);

            await ExecuteDeleteTrigger(connection, model, txn, user);
        }

        private async Task DeleteInnerAsync<TModel>(IDbConnection connection, IDbTransaction txn, TIdentity id)
        {
            var cmd = new CommandDefinition(GetDeleteStatement(typeof(TModel)), new { id }, txn);
            Debug.Print(cmd.CommandText);

            try
            {
                await connection.ExecuteAsync(cmd);
            }
            catch (Exception exc)
            {
                throw new CrudException(cmd, exc);
            }
        }

        private async Task<TModel> AllowDeleteAsync<TModel>(IDbConnection connection, TIdentity id, IDbTransaction txn, IUserBase user, TModel model = default)
        {
            if (user != null && typeof(TModel).ImplementsAny(typeof(ITenantIsolated<TIdentity>), typeof(ITrigger)))
            {
                if (model == null)
                {
                    model = await GetAsync<TModel>(connection, id, txn);
                }

                await VerifyTenantIsolation(connection, user, model, txn);
            }

            return model;
        }

        private async static Task ExecuteDeleteTrigger<TModel>(IDbConnection connection, TModel model, IDbTransaction txn = null, IUserBase user = null)
        {
            try
            {
                var trigger = model as ITrigger;
                if (trigger != null)
                {
                    await trigger.RowDeletedAsync(connection, txn, user);
                }
            }
            catch (Exception exc)
            {
                throw new TriggerException(exc, TriggerAction.Delete);
            }
        }

        public async Task<bool> ExistsAsync<TModel>(IDbConnection connection, TIdentity id, IDbTransaction txn = null)
        {
            var model = await GetAsync<TModel>(connection, id, txn);
            return (model != null);
        }

        public async Task<bool> ExistsWhereAsync<TModel>(IDbConnection connection, object criteria, IDbTransaction txn = null)
        {
            var model = await GetWhereAsync<TModel>(connection, criteria, txn);
            return (model != null);
        }

        private static async Task ValidateInternal<TModel>(IDbConnection connection, TModel model, IDbTransaction txn = null)
        {
            if (typeof(TModel).Implements(typeof(IValidate)))
            {
                var result = ((IValidate)model).Validate();
                if (!result.IsValid) throw new Exceptions.ValidationException(result.Message);

                result = await ((IValidate)model).ValidateAsync(connection, txn);
                if (!result.IsValid) throw new Exceptions.ValidationException(result.Message);
            }
        }

        #region SQL statements        
        public string GetQuerySingleStatement(Type modelType)
        {
            bool isCustom = modelType.Implements(typeof(ICustomGet));

            string query = (isCustom) ?
                GetCustomSelectFrom(modelType) :
                $"SELECT * FROM {SqlBuilder.ApplyDelimiter(modelType.GetTableName(), StartDelimiter, EndDelimiter)}";

            string whereId = (isCustom) ?
                GetCustomWhereId(modelType) :
                $"{SqlBuilder.ApplyDelimiter(modelType.GetIdentityName(), StartDelimiter, EndDelimiter)}=@id";

            return $"{query} WHERE {whereId}";
        }

        public string GetQuerySingleWhereStatement(Type modelType, object criteria)
        {
            var properties = criteria.GetType().GetProperties();
            return GetQuerySingleWhereStatement(modelType, properties);
        }

        public string GetQuerySingleWhereStatement(Type modelType, IEnumerable<string> propertyNames)
        {
            string whereClause = $"WHERE {string.Join(" AND ", propertyNames.Select(name => SqlBuilder.ApplyDelimiter(name, StartDelimiter, EndDelimiter) + "=@" + name))}";

            string query = (modelType.Implements(typeof(ICustomGet))) ?
                GetCustomSelectFrom(modelType) :
                $"SELECT * FROM {SqlBuilder.ApplyDelimiter(modelType.GetTableName(), StartDelimiter, EndDelimiter)}";

            return $"{query} {whereClause}";
        }

        private string GetCustomSelectFrom(Type modelType)
        {
            var model = Activator.CreateInstance(modelType) as ICustomGet;
            return model.SelectFrom;
        }

        private string GetCustomWhereId(Type modelType)
        {
            var model = Activator.CreateInstance(modelType) as ICustomGet;
            return model.WhereId;
        }

        public string GetQuerySingleWhereStatement(Type modelType, IEnumerable<PropertyInfo> properties)
        {
            return GetQuerySingleWhereStatement(modelType, properties.Select(pi => pi.GetColumnName()));
        }

        public string GetInsertStatement(Type modelType, IEnumerable<string> columnNames = null, bool getIdentity = true) => 
            SqlBuilder.Insert(modelType, columnNames, StartDelimiter, EndDelimiter) + ((getIdentity) ? " " + SelectIdentityCommand : string.Empty);

        public string GetUpdateStatement<TModel>(ChangeTracker<TModel> changeTracker = null, IEnumerable<string> columnNames = null)
        {
            var columns =
                columnNames ??
                changeTracker?.GetModifiedColumns(SaveAction.Update);

            return SqlBuilder.Update(typeof(TModel), columns, StartDelimiter, EndDelimiter);
        }

        public string GetDeleteStatement(Type modelType) => SqlBuilder.Delete(modelType, StartDelimiter, EndDelimiter);                
        #endregion

        private static async Task VerifyGetPermission<TModel>(IDbConnection connection, TIdentity identity, IDbTransaction txn, IUserBase user, TModel result)
        {
            var permission = result as IPermission;
            if (permission != null)
            {
                if (!await permission.AllowGetAsync(connection, user, txn))
                {
                    throw new PermissionException($"User {user.Name} does not have permission to {typeof(TModel).Name} Id {identity}");
                }
            }
        }

        private static async Task VerifyTenantIsolation<TModel>(IDbConnection connection, IUserBase user, TModel result, IDbTransaction txn)
        {
            if (user == null) return;

            var isolated = result as ITenantIsolated<TIdentity>;
            var tenantUser = user as ITenantUser<TIdentity>;
            if (isolated != null && tenantUser != null)
            {
                var tenantId = await isolated.GetTenantIdAsync(connection, txn);
                if (!tenantId.Equals(tenantUser.TenantId)) throw new TenantIsolationException($"User {user.Name} is not a valid tenant of {typeof(TModel).Name} Id {tenantId}");
            }
        }

        private static void AuditRow<TModel>(TModel model, SaveAction saveAction, IUserBase user)
        {
            if (user == null) return;
            var audit = model as IAudit;
            audit?.Stamp(saveAction, user);
        }
    }
}
