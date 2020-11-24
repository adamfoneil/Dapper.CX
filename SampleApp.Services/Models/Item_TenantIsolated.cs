using AO.Models.Interfaces;
using System.Data;
using System.Threading.Tasks;

namespace SampleApp.Models
{
    public partial class Item : IDbTenantIsolated<int>
    {
        public async Task<int> GetTenantIdAsync(IDbConnection connection, IDbTransaction txn = null) => await Task.FromResult(WorkspaceId);
    }
}
