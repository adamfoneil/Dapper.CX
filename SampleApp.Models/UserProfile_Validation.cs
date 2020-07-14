using AO.Models;
using AO.Models.Interfaces;
using Dapper.CX.SqlServer.Extensions.Int;
using SampleApp.Models.Queries;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace SampleApp.Models
{
    public partial class UserProfile : IValidate
    {
        public ValidateResult Validate()
        {
            return new ValidateResult() { IsValid = true };
        }

        public async Task<ValidateResult> ValidateAsync(IDbConnection connection, IDbTransaction txn = null)
        {
            if (WorkspaceId.HasValue)
            {
                var validWs =
                    (await new WorkspaceUsers() { UserId = UserId }
                    .ExecuteAsync(connection, txn))
                    .Select(wsu => wsu.WorkspaceId);

                var ws = await connection.GetAsync<Workspace>(WorkspaceId.Value, txn);

                if (!validWs.Contains(WorkspaceId.Value))
                {
                    return new ValidateResult() 
                    { 
                        IsValid = false, 
                        Message = $"User {UserName} does not belong to workspace '{ws.Name}'" 
                    };
                }
            }

            return new ValidateResult() { IsValid = true };
        }
    }
}
