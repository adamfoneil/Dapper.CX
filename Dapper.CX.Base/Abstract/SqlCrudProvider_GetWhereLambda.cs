using AO.Models.Interfaces;
using AO.Models.Static;
using Dapper.CX.Interfaces;
using System;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Dapper.CX.Abstract
{
    public abstract partial class SqlCrudProvider<TIdentity> : ISqlCrudProvider<TIdentity>
    {
        public async Task<TModel> GetWhereAsync<TModel>(IDbConnection connection, IUserBase user, params Expression<Func<TModel, bool>>[] whereExpressions)
        {
            return await GetWhereAsync(connection, whereExpressions, user: user);
        }

        public async Task<TModel> GetWhereAsync<TModel>(IDbConnection connection, IDbTransaction txn, params Expression<Func<TModel, bool>>[] whereExpressions)
        {
            return await GetWhereAsync(connection, whereExpressions, txn: txn);
        }

        public async Task<TModel> GetWhereAsync<TModel>(IDbConnection connection, IUserBase user, IDbTransaction txn, params Expression<Func<TModel, bool>>[] whereExpressions)
        {
            return await GetWhereAsync(connection, whereExpressions, user, txn);
        }

        public async Task<TModel> GetWhereAsync<TModel>(IDbConnection connection, params Expression<Func<TModel, bool>>[] whereExpressions)
        {
            return await GetWhereAsync(connection, whereExpressions, null, null);
        }

        public async Task<TModel> GetWhereAsync<TModel>(IDbConnection connection, Expression<Func<TModel, bool>>[] whereExpressions, IUserBase user = null, IDbTransaction txn = null)
        {
            var cmd = GetWhereClauseCommand(whereExpressions, txn);
            var result = await connection.QuerySingleOrDefaultAsync<TModel>(cmd);

            if (result != null && user != null)
            {
                await VerifyGetPermission(connection, GetIdentity(result), txn, user, result);
                await VerifyTenantIsolation(connection, user, result, txn);                
            }

            await OnGetRelatedAsync(connection, result, txn);

            return result;
        }

        private CommandDefinition GetWhereClauseCommand<TModel>(Expression<Func<TModel, bool>>[] whereExpressions, IDbTransaction txn = null)
        {
            var type = typeof(TModel);
            DynamicParameters dp = new DynamicParameters();

            string whereClause = string.Join(" AND ", whereExpressions.Select(e =>
            {
                var result = WhereClauseExpression(e);
                dp.Add(result.columnName, result.paramValue);
                return $"{SqlBuilder.ApplyDelimiter(result.columnName, StartDelimiter, EndDelimiter)}=@{result.columnName}";
            }));

            string cmdText = $"SELECT * FROM {SqlBuilder.ApplyDelimiter(type.GetTableName(), StartDelimiter, EndDelimiter)} WHERE {whereClause}";            

            Debug.Print(cmdText);

            return new CommandDefinition(cmdText, dp, transaction: txn);
        }

        private (string columnName, object paramValue) WhereClauseExpression(Expression expression)
        {
            var lambdaExp = expression as LambdaExpression;
            if (lambdaExp == null) throw new ArgumentException(nameof(expression));

            var binaryExp = lambdaExp.Body as BinaryExpression;
            if (binaryExp == null) throw new ArgumentException(nameof(binaryExp));

            var left = binaryExp.Left as MemberExpression;
            var right = binaryExp.Right as ConstantExpression;
            
            return (left.Member.Name, right.Value);
        }
    }
}
