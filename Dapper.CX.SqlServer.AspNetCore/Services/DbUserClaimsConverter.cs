using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Dapper.CX.SqlServer.Services
{
    /// <summary>
    /// service for converting Claims to a TUser and back
    /// </summary>
    public abstract class DbUserClaimsConverter<TUser> where TUser : new()
    {
        public abstract Task<TUser> QueryUserAsync(string userName);

        public abstract IEnumerable<Claim> GetClaimsFromUser(TUser user);

        public abstract TUser GetUserFromClaims(string userName, IEnumerable<Claim> claims);

        protected static TUser Parse(IEnumerable<Claim> claims)
        {
            var supportedTypes = new Dictionary<Type, Func<string, object>>()
            {
                [typeof(string)] = (value) => value,
                [typeof(int)] = (value) => Convert.ToInt32(value),
                [typeof(int?)] = (value) => !string.IsNullOrEmpty(value) ? Convert.ToInt32(value) : 0,
                [typeof(bool)] = (value) => Convert.ToBoolean(value),
                [typeof(long)] = (value) => Convert.ToInt64(value),
                [typeof(long?)] = (value) => !string.IsNullOrEmpty(value) ? Convert.ToInt64(value) : 0
            };

            var result = new TUser();
            var props = typeof(TUser).GetProperties().Where(pi => supportedTypes.ContainsKey(pi.PropertyType));
            HashSet<string> propertyNames = props.Select(pi => pi.Name).ToHashSet();

            // for duplicate claim types, assume the last one. If this is bad behavior in your app, 
            // then you can manually parse claims in your GetUserFromClaims implementation
            var claimValues = claims
                .Where(c => propertyNames.Contains(c.Type))
                .ToLookup(c => c.Type)
                .ToDictionary(c => c.Key, c => c.Last());
                
            foreach (var pi in props.Where(pi => claimValues.ContainsKey(pi.Name)))
            {
                pi.SetValue(result, supportedTypes[pi.PropertyType].Invoke(claimValues[pi.Name].Value));
            }

            return result;
        }

        public async Task UpdateClaimsAsync(
            string userName, UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signinManager, 
            IEnumerable<Claim> oldClaims)
        {            
            var identityUser = await userManager.FindByNameAsync(userName);
            await userManager.RemoveClaimsAsync(identityUser, oldClaims);
            await signinManager.RefreshSignInAsync(identityUser);
        }
    }
}
