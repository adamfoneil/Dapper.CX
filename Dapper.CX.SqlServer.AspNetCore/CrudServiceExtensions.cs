using AO.Models.Interfaces;
using Dapper.CX.SqlServer.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Dapper.CX.SqlServer.AspNetCore
{
    public static class CrudServiceExtensions
    {
        public static void AddDapperCXInt<TUser>(this IServiceCollection services, string connectionString) where TUser : IUserBase
        {
            services.AddHttpContextAccessor();
            services.AddScoped((sp) =>
            {
                var http = sp.GetRequiredService<IHttpContextAccessor>();
                return new SqlServerIntCrudService<TUser>(connectionString, http.HttpContext.User.Identity.Name);                
            });
        }

        public static void AddDapperCXLong<TUser>(this IServiceCollection services, string connectionString) where TUser : IUserBase
        {
            services.AddHttpContextAccessor();
            services.AddScoped((sp) =>
            {
                var http = sp.GetRequiredService<IHttpContextAccessor>();
                return new SqlServerLongCrudService<TUser>(connectionString, http.HttpContext.User.Identity.Name);
            });
        }
    }
}
