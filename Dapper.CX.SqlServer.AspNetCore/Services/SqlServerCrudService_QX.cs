using Dapper.QX;
using Microsoft.AspNetCore.Mvc.Rendering;
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

        public async Task<SelectList> QuerySelectListAsync(Query<SelectListItem> query, object selectedValue = null)
        {
            using (var cn = GetConnection())
            {
                var items = await query.ExecuteAsync(cn);
                return new SelectList(items, "Value", "Text", selectedValue);
            }
        }
    }
}
