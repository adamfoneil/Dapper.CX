using System.Data;

namespace Dapper.CX.Abstract
{
    public abstract partial class SqlCrudProvider<TIdentity>
    {
        public TModel Get<TModel>(IDbConnection connection, TIdentity identity)
        {
            return connection.QuerySingleOrDefault<TModel>(GetQuerySingleStatement(typeof(TModel)), new { id = identity });
        }
    }
}
