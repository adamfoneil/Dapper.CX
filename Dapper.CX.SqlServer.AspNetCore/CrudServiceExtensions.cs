using AO.Models.Interfaces;
using Dapper.CX.Abstract;
using Dapper.CX.SqlServer.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;

namespace Dapper.CX.SqlServer.AspNetCore
{
    public static class CrudServiceExtensions
    {
        public static void AddDapperCX<TIdentity, TUser>(this IServiceCollection services, string connectionString) where TUser : IUserBase
        {
            var supportedTypes = new Dictionary<Type, SqlCrudService<TIdentity, TUser>>()
            {

            };

            services.AddHttpContextAccessor();
            services.AddScoped((sp) =>
            {
                var http = sp.GetRequiredService<IHttpContextAccessor>();
                return new SqlServerIntCrudService<TUser>(connectionString, http.HttpContext.User.Identity.Name);                
            });
        }
    }
}
