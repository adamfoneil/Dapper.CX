using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Dapper.CX.SqlServer.Services
{
    /// <summary>
    /// service for converting Claims to a TUser and back
    /// </summary>
    public abstract class DbUserClaimConverter<TUser>
    {
        public abstract Task<TUser> QueryUserAsync(string userName);

        public abstract IEnumerable<Claim> GetClaims(TUser user);

        public abstract TUser GetUser(string userName, IEnumerable<Claim> claims);

        protected static Dictionary<string, string> Parse(IEnumerable<Claim> claims, params string[] types) => claims
            .Where(c => types.Contains(c.Type))
            .ToDictionary(c => c.Type, c => c.Value);
    }
}
