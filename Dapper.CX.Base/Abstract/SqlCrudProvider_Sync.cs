using AO.DbSchema.Attributes.Interfaces;
using AO.DbSchema.Enums;
using Dapper.CX.Classes;
using Dapper.CX.Exceptions;
using Dapper.CX.Extensions;
using System;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq.Expressions;

namespace Dapper.CX.Abstract
{
    public abstract partial class SqlCrudProvider<TIdentity>
    {
        [Category(CrudCategory)]
        public TModel Get<TModel>(IDbConnection connection, TIdentity identity, IDbTransaction txn = null)
        {
            return connection.QuerySingleOrDefault<TModel>(GetQuerySingleStatement(typeof(TModel)), new { id = identity }, txn);
        }

        [Category(CrudCategory)]
        public TModel GetWhere<TModel>(IDbConnection connection, object criteria, IDbTransaction txn = null)
        {
            return connection.QuerySingleOrDefault<TModel>(GetQuerySingleWhereStatement(typeof(TModel), criteria), criteria, txn);
        }

        [Category(CrudCategory)]
        public void Delete<TModel>(IDbConnection connection, TIdentity id, IDbTransaction txn = null)
        {
            var cmd = new CommandDefinition(GetDeleteStatement(typeof(TModel)), new { id }, txn);

            Debug.Print(cmd.CommandText);

            try
            {
                connection.Execute(cmd);
            }
            catch (Exception exc)
            {
                throw new CrudException(cmd, exc);
            }
        }

        [Category(CrudCategory)]
        public TIdentity Insert<TModel>(IDbConnection connection, TModel model, Action<TModel, SaveAction> onSave = null, bool getIdentity = true, IDbTransaction txn = null)
        {
            SyncValidateInner(model);

            onSave?.Invoke(model, SaveAction.Insert);
            var cmd = new CommandDefinition(GetInsertStatement(typeof(TModel), getIdentity: getIdentity), model, txn);

            Debug.Print(cmd.CommandText);

            try
            {
                TIdentity result = connection.QuerySingleOrDefault<TIdentity>(cmd);
                if (getIdentity) SetIdentity(model, result);
                return result;
            }
            catch (Exception exc)
            {
                throw new CrudException(cmd, exc);
            }
        }

        [Category(CrudCategory)]
        public void Update<TModel>(IDbConnection connection, TModel model, ChangeTracker<TModel> changeTracker = null, Action<TModel, SaveAction> onSave = null, IDbTransaction txn = null)
        {
            SyncValidateInner(model);

            onSave?.Invoke(model, SaveAction.Update);
            var cmd = new CommandDefinition(GetUpdateStatement(changeTracker), model, txn);

            Debug.Print(cmd.CommandText);

            try
            {
                connection.Execute(cmd);
            }
            catch (Exception exc)
            {
                throw new CrudException(cmd, exc);
            }
        }

        [Category(CrudCategory)]
        public void Update<TModel>(IDbConnection connection, TModel @object, IDbTransaction txn, params Expression<Func<TModel, object>>[] setColumns)
        {
            CommandDefinition cmd = GetSetColumnsUpdateCommand(@object, setColumns, txn);            
            connection.Execute(cmd);
        }

        [Category(CrudCategory)]
        public void Update<TModel>(IDbConnection connection, TModel @object, params Expression<Func<TModel, object>>[] setColumns)
        {
            CommandDefinition cmd = GetSetColumnsUpdateCommand(@object, setColumns);
            connection.Execute(cmd);
        }

        [Category(CrudCategory)]
        public TIdentity Save<TModel>(IDbConnection connection, TModel model, ChangeTracker<TModel> changeTracker = null, Action<TModel, SaveAction> onSave = null, IDbTransaction txn = null)
        {
            if (IsNew(model))
            {
                return Insert(connection, model, onSave, txn: txn);
            }
            else
            {
                Update(connection, model, changeTracker, onSave, txn);
                return GetIdentity(model);
            }
        }

        private static void SyncValidateInner<TModel>(TModel model)
        {
            if (typeof(TModel).Implements(typeof(IValidate)))
            {
                var result = ((IValidate)model).Validate();
                if (!result.IsValid) throw new ValidationException(result.Message);
            }
        }
    }
}
