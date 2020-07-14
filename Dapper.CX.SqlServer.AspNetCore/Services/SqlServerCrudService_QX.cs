using Dapper.QX;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dapper.CX.SqlServer.Services
{
    public partial class SqlServerCrudService<TIdentity, TUser>
    {
        public async Task<IEnumerable<TResult>> QueryAsync<TResult>(Query<TResult> query)
        {
            using (var cn = GetConnection())
            {
                return await query.ExecuteAsync(cn);
            }
        }

        public async Task<TResult> QuerySingleAsync<TResult>(Query<TResult> query)
        {
            using (var cn = GetConnection())
            {
                return await query.ExecuteSingleAsync(cn);
            }
        }

        public async Task<TResult> QuerySingleOrDefaultAsync<TResult>(Query<TResult> query)
        {
            using (var cn = GetConnection())
            {
                return await query.ExecuteSingleOrDefaultAsync(cn);
            }
        }
    }
}
