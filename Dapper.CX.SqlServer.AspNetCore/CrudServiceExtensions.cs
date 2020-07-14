using AO.Models.Interfaces;
using Dapper.CX.SqlServer.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Dapper.CX.SqlServer.AspNetCore
{
    public static class CrudServiceExtensions
    {
        public static void AddDapperCX<TIdentity, TUser>(this IServiceCollection services, string connectionString, Func<object, TIdentity> convertIdentity) where TUser : IUserBase
        {
            services.AddHttpContextAccessor();
            services.AddScoped((sp) =>
            {
                var http = sp.GetRequiredService<IHttpContextAccessor>();
                return new SqlServerCrudService<TIdentity, TUser>(connectionString, http.HttpContext.User.Identity.Name, convertIdentity);
            });
        }
    }
}
