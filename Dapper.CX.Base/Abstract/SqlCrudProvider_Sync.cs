using AO.DbSchema.Enums;
using Dapper.CX.Exceptions;
using System;
using System.Data;

namespace Dapper.CX.Abstract
{
    public abstract partial class SqlCrudProvider<TIdentity>
    {
        public TModel Get<TModel>(IDbConnection connection, TIdentity identity)
        {
            return connection.QuerySingleOrDefault<TModel>(GetQuerySingleStatement(typeof(TModel)), new { id = identity });
        }

        public TModel GetWhere<TModel>(IDbConnection connection, object criteria)
        {
            return connection.QuerySingleOrDefault<TModel>(GetQuerySingleWhereStatement(typeof(TModel), criteria), criteria);
        }

        public TIdentity Insert<TModel>(IDbConnection connection, TModel model, Action<TModel, SaveAction> onSave = null, bool getIdentity = true)
        {
            onSave?.Invoke(model, SaveAction.Insert);
            var cmd = new CommandDefinition(GetInsertStatement(typeof(TModel), getIdentity: getIdentity), model);

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
