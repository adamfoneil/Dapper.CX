using AO.Models.Interfaces;
using Dapper.CX.Classes;
using Dapper.CX.SqlServer.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Security.Claims;

namespace Dapper.CX.SqlServer.AspNetCore
{
    public static class CrudServiceExtensions
    {
        /// <summary>
        /// Dapper.CX config method that lets you use your own derived SqlServerCrudService class
        /// </summary>
        public static void AddDapperCX<TIdentity, TUser, TService>(
            this IServiceCollection services,
            Func<DbUserClaimsConverter<TUser>> claimsConverterFactory,
            Func<IServiceProvider, TService> serviceFactory)
            where TUser : IUserBase, new()
            where TService : SqlServerCrudService<TIdentity, TUser>, new()
        {
            services.AddHttpContextAccessor();
            services.AddSingleton(claimsConverterFactory.Invoke());     
            services.AddScoped((sp) => serviceFactory.Invoke(sp));
        }

        
        /// <summary>
        /// Dapper.CX config method that uses the built-in SqlServerCrudService class
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
                return new SqlServerCrudService<TIdentity, TUser>(connectionString, context.user, convertIdentity)
                {
                    OnUserUpdatedAsync = async (user) =>
                    {
                        await context.claimsConverter.UpdateClaimsAsync(user.Name, context.userManager, context.signinManager, context.claims);
                    }
                };                
            });
        }

        /// <summary>
        /// simplest Dapper.CX use case, with no user profile integation
        /// </summary>
        public static void AddDapperCXBasic<TIdentity>(
            this IServiceCollection services, 
            string connectionString, Func<object, TIdentity> convertIdentity, string systemUserName = "system")
        {
            services.AddScoped((sp) =>
            {
                return new SqlServerCrudService<TIdentity, SystemUser>(connectionString, new SystemUser(systemUserName), convertIdentity);
            });
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
    }
}
