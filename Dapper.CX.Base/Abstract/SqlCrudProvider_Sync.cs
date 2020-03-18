using AO.DbSchema.Enums;
using Dapper.CX.Exceptions;
using System;
using System.Data;

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

            try
            {
                connection.Execute(cmd);
            }
            catch (Exception exc)
            {
                throw new CrudException(cmd, exc);
            }
        }

        public TIdentity Insert<TModel>(IDbConnection connection, TModel model, Action<TModel, SaveAction> onSave = null, bool getIdentity = true, IDbTransaction txn = null)
        {
            onSave?.Invoke(model, SaveAction.Insert);
            var cmd = new CommandDefinition(GetInsertStatement(typeof(TModel), getIdentity: getIdentity), model, txn);

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
    }
}
