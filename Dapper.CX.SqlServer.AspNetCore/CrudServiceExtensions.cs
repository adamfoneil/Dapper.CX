using AO.Models.Interfaces;
using Dapper.CX.SqlServer.Services;
using Microsoft.AspNetCore.Http;
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
                var http = sp.GetRequiredService<IHttpContextAccessor>();
                var claimConverter = sp.GetRequiredService<DbUserClaimsConverter<TUser>>();
                var user = claimConverter.GetUser(http.HttpContext.User.Identity.Name, http.HttpContext.User.Claims);
                return new SqlServerCrudService<TIdentity, TUser>(connectionString, user, convertIdentity);
            });
        }
    }
}
