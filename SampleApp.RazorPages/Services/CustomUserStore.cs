using Dapper;
using Dapper.CX.SqlServer;
using Dapper.CX.SqlServer.Extensions.Int;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;
using SampleApp.Models;
using SampleApp.RazorPages.Data;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SampleApp.RazorPages.Services
{
    public class CustomUserStore : UserStore<IdentityUser>
    {
        private readonly string _connectionString;

        public CustomUserStore(string connectionString, ApplicationDbContext context, IdentityErrorDescriber describer) : base(context, describer)
        {
            _connectionString = connectionString;
        }

        public override async Task<IdentityResult> CreateAsync(IdentityUser user, CancellationToken cancellationToken = default)
        {
            var result = await base.CreateAsync(user, cancellationToken);

            if (result.Succeeded)
            {
                await CreateDefaultWorkspaceUserAsync(user);
            }

            return result;
        }

        /// <summary>
        /// associates a newly-created user with the built in "default" workspace so that
        /// we can demo tenant-specific features without additional registration steps
        /// </summary>
        private async Task CreateDefaultWorkspaceUserAsync(IdentityUser user)
        {
            const string defaultWsName = "default";

            using (var cn = new SqlConnection(_connectionString))
            {
                var ws = await cn.GetWhereAsync<Workspace>(new { name = defaultWsName });
                var userId = await cn.QuerySingleAsync<int>("SELECT [UserId] FROM [dbo].[AspNetUsers] WHERE [UserName]=@userName", new { user.UserName });

                // all new users will be enabled in the "default" workspace
                var wsUser = new WorkspaceUser()
                {
                    WorkspaceId = ws.Id,
                    UserId = userId,
                    Status = UserStatus.Enabled,
                    CreatedBy = "system",
                    DateCreated = DateTime.UtcNow
                };
                await cn.SaveAsync(wsUser);

                // and we need to update the user row with the correct workspace Id
                await new SqlServerCmd("dbo.AspNetUsers", "UserId")
                {
                    ["WorkspaceId"] = ws.Id
                }.UpdateAsync(cn, userId);
            }
        }
    }

    public static partial class ServiceExtensions
    {
        public static void AddCustomUserStore(this IServiceCollection services, string connectionString)
        {
            services.AddTransient<IUserStore<IdentityUser>>((sp) =>
            {
                var context = sp.GetRequiredService<ApplicationDbContext>();
                var describer = sp.GetRequiredService<IdentityErrorDescriber>();
                return new CustomUserStore(connectionString, context, describer);
            });
        }
    }
}
