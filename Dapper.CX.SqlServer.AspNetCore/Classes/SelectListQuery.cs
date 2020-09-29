using Dapper.QX;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace Dapper.CX.SqlServer.AspNetCore.Classes
{
    public class SelectListQuery : Query<SelectListItem>
    {
        public SelectListQuery(string sql) : base(sql)
        {
        }

        public async Task<SelectList> ExecuteSelectListAsync(IDbConnection connection, object selectedValue = null)
        {
            return await ExecuteInternalAsync(this, connection, selectedValue);
        }

        internal static async Task<SelectList> ExecuteInternalAsync(Query<SelectListItem> query, IDbConnection connection, object selectedValue = null)
        {
            IEnumerable<SelectListItem> items = await query.ExecuteAsync(connection);
            return new SelectList(items, nameof(SelectListItem.Value), nameof(SelectListItem.Text), selectedValue);
        }
    }
}
