using AO.Models.Interfaces;
using Dapper.CX.SqlServer.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Dapper.CX.SqlServer.AspNetCore
{
    public static class CrudServiceExtensions
    {
        public static void AddDapperCX<TIdentity, TUser>(
            this IServiceCollection services, 
            string connectionString, Func<DbUserClaimsConverter<TUser>> claimConverterFactory, Func<object, TIdentity> convertIdentity) where TUser : IUserBase, new()            
        {
            services.AddSingleton(claimConverterFactory.Invoke());
            services.AddHttpContextAccessor();
            services.AddScoped((sp) =>
            {
                var signinManager = sp.GetRequiredService<SignInManager<IdentityUser>>();                
                var userManager = sp.GetRequiredService<UserManager<IdentityUser>>();

                var http = sp.GetRequiredService<IHttpContextAccessor>();
                var claimConverter = sp.GetRequiredService<DbUserClaimsConverter<TUser>>();
                var user = claimConverter.GetUserFromClaims(http.HttpContext.User.Identity.Name, http.HttpContext.User.Claims);
                return new SqlServerCrudService<TIdentity, TUser>(connectionString, user, convertIdentity)
                {
                    OnUserUpdatedAsync = async (user) =>
                    {                        
                        await claimConverter.UpdateClaimsAsync(user.Name, userManager, signinManager, http.HttpContext.User.Claims);
                    }
                };
            });
        }
    }
}
