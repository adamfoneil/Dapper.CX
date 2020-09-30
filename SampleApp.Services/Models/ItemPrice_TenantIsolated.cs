using AO.Models;
using AO.Models.Interfaces;
using Dapper.CX.SqlServer.Extensions.Int;
using System.Data;
using System.Threading.Tasks;

namespace SampleApp.Models
{
    public partial class ItemPrice : IValidate, ITenantIsolated<int>, IGetRelated
    {
        /// <summary>
        /// a "navigation property" to a related row
        /// </summary>
        public Item Item { get; set; }

        /// <summary>
        /// whenever we get an ItemPrice, we automatically get the related Item
        /// </summary>
        public async Task GetRelatedAsync(IDbConnection connection, IDbTransaction txn = null)
        {
            // uses the Dapper.CX GetAsync extension method
            Item = await connection.GetAsync<Item>(ItemId, txn);
        }

        public async Task<int> GetTenantIdAsync(IDbConnection connection, IDbTransaction txn = null)
        {
            // if our navigation property to the Item is set, we can return its WorkspaceId
            if (Item != null) return Item.WorkspaceId;

            // we (probably) can't always assume our nav property is set, so we'll set now if not
            await GetRelatedAsync(connection, txn);

            // now we can return the tenantId, which is the workspaceId
            return Item.WorkspaceId;
        }
    }
}
