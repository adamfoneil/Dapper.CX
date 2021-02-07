using AO.Models.Interfaces;
using Dapper.CX.Models;
using Dapper.CX.SqlServer.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace Dapper.CX.SqlServer.AspNetCore
{
    public static class CrudServiceExtensions
    {                
        /// <summary>
        /// Dapper.CX config method that gives user profile integration with local accounts
        /// </summary>
        public static void AddDapperCX<TIdentity, TUser>(
            this IServiceCollection services,
            string connectionString,
            Func<object, TIdentity> convertIdentity, 
            Func<DbUserClaimsConverter<TUser>> claimsConverterFactory)
            where TUser : IUserBase, new()            
        {
            services.AddHttpContextAccessor();
            services.AddSingleton(claimsConverterFactory.Invoke());            
            services.AddScoped((sp) =>
            {
                var context = sp.GetDapperCXContext<TUser>();
                return new DapperCX<TIdentity, TUser>(connectionString, context.user, convertIdentity)
                {
                    OnUserUpdatedAsync = async (user) =>
                    {
                        await context.claimsConverter.UpdateClaimsAsync(user.Name, context.userManager, context.signinManager, context.claims);
                    }
                };                
            });
        }

        public static void AddDapperCX<TIdentity, TUser>(
            this IServiceCollection services,
            string connectionString,  
            Func<IServiceProvider, TUser> getUser,
            Func<object, TIdentity> convertIdentity) where TUser : IUserBase, new()
        {            
            services.AddScoped((sp) =>
            {
                var user = getUser.Invoke(sp);
                return new DapperCX<TIdentity, TUser>(connectionString, user, convertIdentity);
            });
        }

        /// <summary>
        /// simplest Dapper.CX use case, with no user profile integation
        /// </summary>
        public static void AddDapperCX<TIdentity>(
            this IServiceCollection services, 
            string connectionString, Func<object, TIdentity> convertIdentity)
        {
            services.AddScoped((sp) =>
            {                
                return new DapperCX<TIdentity>(connectionString, convertIdentity);
            });
        }

        public static void AddChangeTracking(this IServiceCollection services, string connectionString, ISqlObjectCreator objectCreator)
        {
            using (var cn = new SqlConnection(connectionString))
            {
                var commands = objectCreator.GetStatementsAsync(cn, new Type[]
                {
                    typeof(ColumnHistory),
                    typeof(RowVersion)
                }).Result;

                foreach (var cmd in commands) cn.Execute(cmd);
            }
        }

        /// <summary>
        /// Helper method that extracts the useful things from the service provider to configure Dapper.CX during startup.
        /// This is intended for use in your own serviceFactory method so you can implement user profile integration
        /// </summary>
        public static (
            TUser user,
            DbUserClaimsConverter<TUser> claimsConverter,
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signinManager,
            IEnumerable<Claim> claims
        ) GetDapperCXContext<TUser>(this IServiceProvider serviceProvider) where TUser : IUserBase, new()
        {
            var signinManager = serviceProvider.GetRequiredService<SignInManager<IdentityUser>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();
            var claimsFactory = signinManager.ClaimsFactory as DbUserClaimsFactory<TUser>;
            var http = serviceProvider.GetRequiredService<IHttpContextAccessor>();
            var claimsConverter = claimsFactory.ClaimsConverter;
            return (
                claimsConverter.GetUserFromClaims(http.HttpContext.User.Identity.Name, http.HttpContext.User.Claims),
                claimsConverter,
                userManager,
                signinManager,
                http.HttpContext.User.Claims
            );
        }

        public static string GetUserName(this IServiceProvider serviceProvider)
        {
            string userName;

            try
            {
                // blazor components use this
                var authState = serviceProvider.GetRequiredService<AuthenticationStateProvider>();
                var user = authState.GetAuthenticationStateAsync().Result;
                userName = user.User.Identity.Name;
            }
            catch
            {
                // razor pages use this
                var contextAccessor = serviceProvider.GetRequiredService<IHttpContextAccessor>();
                userName = contextAccessor.HttpContext.User.Identity.Name;
            }

            return userName;
        }

        public static TUser GetAspNetUserWithRoles<TUser>(this IServiceProvider serviceProvider, string connectionString, string sessionKey = null) where TUser : IUserBaseWithRoles
        {
            return GetAspNetUserInner<TUser>(serviceProvider, connectionString, (cn, user) =>
            {
                user.Roles = cn.Query<string>(
                    @"SELECT 
			            [r].[Name]
		            FROM 
			            [dbo].[AspNetRoles] [r]
			            INNER JOIN [dbo].[AspNetUserRoles] [ur] ON [r].[Id]=[ur].[RoleId]
			            INNER JOIN [dbo].[AspNetUsers] [u] ON [ur].[UserId]=[u].[Id]
		            WHERE
			            [u].[UserName]=@userName", new { userName = user.Name }).ToHashSet();
            }, sessionKey);
        }

        public static TUser GetAspNetUser<TUser>(this IServiceProvider serviceProvider, string connectionString, string sessionKey = null) where TUser : IUserBase
        {
            return GetAspNetUserInner<TUser>(serviceProvider, connectionString, sessionKey: sessionKey);
        }

        private static TUser GetAspNetUserInner<TUser>(this IServiceProvider serviceProvider, string connectionString, Action<IDbConnection, TUser> customQueries = null, string sessionKey = null) where TUser : IUserBase
        {
            var userName = GetUserName(serviceProvider);
            if (string.IsNullOrEmpty(userName)) return default;

            TUser user = default;
            ISession session = null;
            bool trySession = !string.IsNullOrEmpty(sessionKey);
            bool useSession = false;

            if (trySession && TryGetSession(out session))
            {
                useSession = true;
                user = GetSessionUser(session);
            }

            if (user == null)
            {
                using (var cn = new SqlConnection(connectionString))
                {
                    user = cn.QuerySingle<TUser>("SELECT * FROM [dbo].[AspNetUsers] WHERE [UserName]=@userName", new { userName });
                    customQueries?.Invoke(cn, user);
                }
            }

            if (useSession && user != null)
            {
                var json = JsonSerializer.Serialize(user);
                session.Set(sessionKey, Encoding.UTF8.GetBytes(json));
            }

            return user;

            bool TryGetSession(out ISession session)
            {
                try
                {
                    session = serviceProvider.GetRequiredService<ISession>();
                    return true;
                }
                catch 
                {
                    session = default;
                    return false;
                }
            }

            TUser GetSessionUser(ISession session)
            {
                try
                {                    
                    byte[] data = null;
                    string json;
                    if (session.TryGetValue(sessionKey, out data))
                    {
                        json = Encoding.UTF8.GetString(data);
                        return JsonSerializer.Deserialize<TUser>(json);                        
                    }

                    return default;
                }
                catch
                {
                    return default;
                }
            }
        }

        private static (bool success, string name) GetUserName(IHttpContextAccessor httpContextAccessor)
        {
            var context = httpContextAccessor.HttpContext;
            try
            {
                return (true, context.User.Identity.Name);
            }
            catch
            {
                const string nameClaim = "preferred_username";
                try
                {
                    var claimsLookup = context.User.Claims.ToLookup(c => c.Type);
                    return (true, claimsLookup[nameClaim].First().Value);
                }
                catch
                {
                    return (false, string.Empty);                    
                }
            }
        }
    }
}
