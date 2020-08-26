using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Dapper.CX.SqlServer.Services
{
    /// <summary>
    /// Queries properties of a user from the database, and converts select properties into claims.
    /// This makes it so successive page requests don't require a roundtrip to the database to get user profile info
    /// </summary>
    public class DbUserClaimFactory<TUser> : UserClaimsPrincipalFactory<IdentityUser>
    {
        private readonly DbUserClaimConverter<TUser> _claimConverter;

        public DbUserClaimFactory(
            UserManager<IdentityUser> userManager,
            IOptions<IdentityOptions> optionsAccessor, 
            DbUserClaimConverter<TUser> claimConverter) : base(userManager, optionsAccessor)
        {
            _claimConverter = claimConverter;
        }
        
        protected async override Task<ClaimsIdentity> GenerateClaimsAsync(IdentityUser user)
        {            
            var result = await base.GenerateClaimsAsync(user);
            var dbUser = await _claimConverter.QueryUserAsync(user.UserName);
            var claims = _claimConverter.GetClaims(dbUser);
            result.AddClaims(claims);
            return result;
        }
    }
}
