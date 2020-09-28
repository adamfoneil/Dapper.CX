using AO.Models;
using AO.Models.Interfaces;
using Dapper.CX.SqlServer.Extensions.Int;
using SampleApp.Models.Queries;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace SampleApp.Models
{
    public partial class UserProfile : IValidate, ITenantUser<int>
    {
        [NotMapped]
        public string WorkspaceName { get; set; }

        [NotMapped]
        public bool IsWorkspaceEnabled { get; set; }

        public int TenantId => WorkspaceId ?? 0;

        public ValidateResult Validate()
        {
            return new ValidateResult() { IsValid = true };
        }

        public async Task<ValidateResult> ValidateAsync(IDbConnection connection, IDbTransaction txn = null)
        {
            var result = new ValidateResult() { IsValid = true };

            if (WorkspaceId.HasValue)
            {
                var validWs =
                    (await new WorkspaceUsers() { UserId = UserId, Status = UserStatus.Enabled }
                    .ExecuteAsync(connection, txn))
                    .Select(wsu => wsu.WorkspaceId);

                var ws = await connection.GetAsync<Workspace>(WorkspaceId.Value, txn);

                if (!validWs.Contains(WorkspaceId.Value))
                {
                    result.IsValid = false;
                    result.Message = $"User {UserName} does not belong to workspace '{ws.Name}'";
                }
            }

            return result;
        }
    }
}
