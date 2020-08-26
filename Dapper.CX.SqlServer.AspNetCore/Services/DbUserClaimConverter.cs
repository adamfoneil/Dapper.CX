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
    public abstract class DbUserClaimConverter<TUser> where TUser : new()
    {
        public abstract Task<TUser> QueryUserAsync(string userName);

        public abstract IEnumerable<Claim> GetClaims(TUser user);

        public abstract TUser GetUser(string userName, IEnumerable<Claim> claims);

        protected static TUser Parse(IEnumerable<Claim> claims)
        {
            var supportedTypes = new Dictionary<Type, Func<string, object>>()
            {
                [typeof(string)] = (value) => value,
                [typeof(int)] = (value) => Convert.ToInt32(value)
            };

            var result = new TUser();
            var props = typeof(TUser).GetProperties().Where(pi => supportedTypes.ContainsKey(pi.PropertyType));
            HashSet<string> propertyNames = props.Select(pi => pi.Name).ToHashSet();

            var claimValues = claims
                .Where(c => propertyNames.Contains(c.Type))
                .ToDictionary(c => c.Type);

            foreach (var pi in props)
            {
                pi.SetValue(result, supportedTypes[pi.PropertyType].Invoke(claimValues[pi.Name].Value));
            }

            return result;
        }
            
    }
}
