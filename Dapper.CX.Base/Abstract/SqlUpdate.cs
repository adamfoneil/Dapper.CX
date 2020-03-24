using Dapper.CX.Extensions;
using System;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Dapper.CX.Abstract
{
    public abstract class SqlUpdate<TModel, TIdentity>
    {
        protected abstract SqlCrudProvider<TIdentity> GetProvider();

        private readonly string _sql;
        private readonly DynamicParameters _params;

        public SqlUpdate(TModel @object, params Expression<Func<TModel, object>>[] expressions)
        {
            var provider = GetProvider();
            var columnNames = expressions.Select(exp => SqlCrudProvider<TIdentity>.PropertyNameFromLambda(exp));
            _sql = provider.GetUpdateStatement<TIdentity>(columnNames: columnNames);

            _params = new DynamicParameters();
            foreach (var exp in expressions)
            {
                var value = exp.Compile().Invoke(@object);
                var name = SqlCrudProvider<TIdentity>.PropertyNameFromLambda(exp);
                _params.Add(name, value);
            }
        }

        public async Task ExecuteAsync(IDbConnection connection, TIdentity id, IDbTransaction txn = null)
        {
            var identity = typeof(TModel).GetIdentityName();
            _params.Add(identity, id);
            await connection.ExecuteAsync(_sql, _params, txn);
        }
    }
}
