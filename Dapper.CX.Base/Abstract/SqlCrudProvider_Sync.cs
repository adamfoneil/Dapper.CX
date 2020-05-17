using AO.Models.Enums;
using AO.Models.Interfaces;
using Dapper.CX.Classes;
using Dapper.CX.Exceptions;
using System;
using System.Data;
using System.Diagnostics;
using System.Linq.Expressions;

namespace Dapper.CX.Abstract
{
    public abstract partial class SqlCrudProvider<TIdentity>
    {        
        public TModel Get<TModel>(IDbConnection connection, TIdentity identity, IDbTransaction txn = null)
        {
            return connection.QuerySingleOrDefault<TModel>(GetQuerySingleStatement(typeof(TModel)), new { id = identity }, txn);
        }
     
        public TModel GetWhere<TModel>(IDbConnection connection, object criteria, IDbTransaction txn = null)
        {
            return connection.QuerySingleOrDefault<TModel>(GetQuerySingleWhereStatement(typeof(TModel), criteria), criteria, txn);
        }

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
        
        public TIdentity Insert<TModel>(IDbConnection connection, TModel model, bool getIdentity = true, IDbTransaction txn = null)
        {
            SyncValidateInner(model);
            
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
        
        public void Update<TModel>(IDbConnection connection, TModel model, ChangeTracker<TModel> changeTracker = null, IDbTransaction txn = null)
        {
            SyncValidateInner(model);
            
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
        
        public void Update<TModel>(IDbConnection connection, TModel @object, IDbTransaction txn, params Expression<Func<TModel, object>>[] setColumns)
        {
            CommandDefinition cmd = GetSetColumnsUpdateCommand(@object, setColumns, txn);
            connection.Execute(cmd);
        }
        
        public void Update<TModel>(IDbConnection connection, TModel @object, params Expression<Func<TModel, object>>[] setColumns)
        {
            CommandDefinition cmd = GetSetColumnsUpdateCommand(@object, setColumns);
            connection.Execute(cmd);
        }
        
        public TIdentity Save<TModel>(IDbConnection connection, TModel model, ChangeTracker<TModel> changeTracker = null, IDbTransaction txn = null)
        {
            if (IsNew(model))
            {
                return Insert(connection, model, txn: txn);
            }
            else
            {
                Update(connection, model, changeTracker, txn);
                return GetIdentity(model);
            }
        }

        private static void SyncValidateInner<TModel>(TModel model)
        {
            var validate = model as IValidate;
            if (validate != null)
            {
                var result = validate.Validate();
                if (!result.IsValid) throw new ValidationException(result.Message);
            }
        }
    }
}
