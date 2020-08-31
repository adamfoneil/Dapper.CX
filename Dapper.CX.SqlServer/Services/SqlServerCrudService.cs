using AO.Models.Interfaces;
using Dapper.CX.Abstract;
using Dapper.CX.Classes;
using Microsoft.Data.SqlClient;
using System;
using System.Data;
using System.Threading.Tasks;

namespace Dapper.CX.SqlServer.Services
{
    public partial class SqlServerCrudService<TIdentity, TUser> : SqlCrudService<TIdentity, TUser> where TUser : IUserBase
    {
        public SqlServerCrudService(string connectionString, TUser user, Func<object, TIdentity> convertIdentity) : base(connectionString, user, new SqlServerCrudProvider<TIdentity>(convertIdentity))
        {
        }

        public override IDbConnection GetConnection() => new SqlConnection(_connectionString);

        public async Task AddRoleAsync(string roleName)
        {
            using (var cn = GetConnection())
            {
                var roleId = await FindOrCreateRoleAsync(cn, roleName);
                var userId = await GetUserIdAsync(cn);

                await new SqlServerCmd("dbo.AspNetUserRoles")
                {
                    ["#UserId"] = userId,
                    ["#RoleId"] = roleId
                }.InsertAsync(cn);
            }
        }

        private async Task<Guid> GetUserIdAsync(IDbConnection cn)
        {
            return await cn.QuerySingleAsync<Guid>(
                "SELECT [Id] FROM [dbo].[AspNetUsers] WHERE [UserName]=@userName", 
                new { userName = User.Name });
        }

        private async Task<Guid> FindOrCreateRoleAsync(IDbConnection cn, string roleName)
        {
            var result = await cn.QuerySingleOrDefaultAsync<Guid>(
                @"SELECT [Id] FROM [dbo].[AspNetRoles] WHERE [Name]=@roleName", new { roleName });

            if (result.Equals(default))
            {
                result = Guid.NewGuid();

                await new SqlServerCmd("dbo.AspNetRoles")
                {
                    ["Name"] = roleName,
                    ["NormalizedName"] = roleName,
                    ["Id"] = result
                }.InsertAsync(cn);
            }

            return result;
        }
    }
}
