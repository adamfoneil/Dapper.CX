using AO.Models.Interfaces;
using AO.Models.Static;
using Dapper.CX.Interfaces;
using System;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace Dapper.CX.Abstract
{
    public abstract partial class SqlCrudProvider<TIdentity> : ISqlCrudProvider<TIdentity>
    {
        public async Task<TModel> GetWhereAsync<TModel>(IDbConnection connection, IUserBase user, params Expression<Func<TModel, object>>[] whereExpressions)
        {
            throw new NotImplementedException();
        }

        public async Task<TModel> GetWhereAsync<TModel>(IDbConnection connection, IDbTransaction txn, params Expression<Func<TModel, object>>[] whereExpressions)
        {
            throw new NotImplementedException();
        }

        public async Task<TModel> GetWhereAsync<TModel>(IDbConnection connection, IUserBase user, IDbTransaction txn, params Expression<Func<TModel, object>>[] whereExpressions)
        {
            throw new NotImplementedException();
        }

        public async Task<TModel> GetWhereAsync<TModel>(IDbConnection connection, params Expression<Func<TModel, object>>[] whereExpressions)
        {
            throw new NotImplementedException();
        }

        public async Task<TModel> GetWhereAsync<TModel>(IDbConnection connection, Expression<Func<TModel, object>>[] whereExpressions, IUserBase user = null, IDbTransaction txn = null)
        {
            throw new NotImplementedException();
        }

        private CommandDefinition GetWhereClauseCommand<TModel>(Expression<Func<TModel, object>>[] whereExpressions, IDbTransaction txn = null)
        {
            var type = typeof(TModel);
            DynamicParameters dp = new DynamicParameters();

            string whereClause = string.Join(", ", whereExpressions.Select(e =>
            {
                string propName = PropertyNameFromLambda(e);
                PropertyInfo pi = type.GetProperty(propName);
                dp.Add(propName, e.Compile().Invoke(@object));
                return $"{SqlBuilder.ApplyDelimiter(pi.GetColumnName(), StartDelimiter, EndDelimiter)}=@{propName}";
            }));

            string cmdText = $"UPDATE {SqlBuilder.ApplyDelimiter(type.GetTableName(), StartDelimiter, EndDelimiter)} SET {whereClause} WHERE {SqlBuilder.ApplyDelimiter(type.GetIdentityName(), StartDelimiter, EndDelimiter)}=@id";
            dp.Add("id", GetIdentity(@object));

            Debug.Print(cmdText);

            return new CommandDefinition(cmdText, dp, transaction: txn);
        }
    }
}
