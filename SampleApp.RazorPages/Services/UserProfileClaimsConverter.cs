using Dapper;
using Dapper.CX.SqlServer.Services;
using Microsoft.Data.SqlClient;
using SampleApp.Models;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SampleApp.RazorPages.Services
{
    public class UserProfileClaimsConverter : DbUserClaimsConverter<UserProfile>
    {
        private readonly string _connectionString;

        private const string roleClaim = "role_name";
        private const string wsIdClaim = "ws_id";

        public UserProfileClaimsConverter(string connectionString)
        {
            _connectionString = connectionString;
        }

        public override IEnumerable<Claim> GetClaimsFromUser(UserProfile user)
        {
            yield return new Claim(nameof(UserProfile.WorkspaceId), (user.WorkspaceId ?? 0).ToString());
            yield return new Claim(nameof(UserProfile.WorkspaceName), user.WorkspaceName);
            yield return new Claim(nameof(UserProfile.TimeZoneId), user.TimeZoneId);
            yield return new Claim(nameof(UserProfile.DisplayName), user.DisplayName);
            yield return new Claim(nameof(UserProfile.UserId), user.UserId.ToString());
            yield return new Claim(nameof(UserProfile.Email), user.Email);
            yield return new Claim(nameof(UserProfile.IsWorkspaceEnabled), user.IsWorkspaceEnabled.ToString());

            foreach (var role in user.Roles) yield return new Claim(roleClaim, role);
            foreach (var id in user.WorkspaceIds) yield return new Claim(wsIdClaim, id.ToString());
        }

        public override UserProfile GetUserFromClaims(string userName, IEnumerable<Claim> claims)
        {
            var result = Parse(claims);
            result.UserName = userName;

            result.Roles = new HashSet<string>();
            foreach (var claim in claims.Where(c => c.Type.Equals(roleClaim))) result.Roles.Add(claim.Value);

            result.WorkspaceIds = claims.Where(c => c.Type.Equals(wsIdClaim)).Select(c => int.Parse(c.Value)).ToArray();

            return result;
        }

        public override async Task<UserProfile> QueryUserAsync(string userName)
        {
            using (var cn = new SqlConnection(_connectionString))
            {
                var result = await cn.QuerySingleAsync<UserProfile>(
                    @"SELECT [u].*, [ws].[Name] AS [WorkspaceName], CASE [wu].[Status] WHEN 2 THEN 1 ELSE 0 END AS [IsWorkspaceEnabled]
                    FROM [dbo].[AspNetUsers] [u] 
                    LEFT JOIN [dbo].[Workspace] [ws] ON [u].[WorkspaceId]=[ws].[Id]
                    LEFT JOIN [dbo].[WorkspaceUser] [wu] ON [u].[UserId]=[wu].[UserId] AND [wu].[WorkspaceId]=[u].[WorkspaceId]
                    WHERE [UserName]=@userName",
                    new { userName });

                result.Roles = (await cn.QueryAsync<string>(
                    @"SELECT [r].[Name]
                    FROM [dbo].[AspNetRoles] [r]
                    INNER JOIN [dbo].[AspNetUserRoles] [ur] ON [r].[Id]=[ur].[RoleId]
                    INNER JOIN [dbo].[AspNetUsers] [u] ON [ur].[UserId]=[u].[Id]
                    WHERE [u].[UserName]=@userName", new { userName })).ToHashSet();

                result.WorkspaceIds = (await cn.QueryAsync<int>(
                    @"SELECT [wu].[WorkspaceId] 
                    FROM [dbo].[WorkspaceUser] [wu]
                    INNER JOIN [dbo].[AspNetUsers] [u] ON [wu].[UserId]=[u].[UserId]
                    WHERE [u].[UserName]=@userName AND [wu].[Status]=2", new { userName })).ToArray();

                return result;
            }
        }
    }
}
