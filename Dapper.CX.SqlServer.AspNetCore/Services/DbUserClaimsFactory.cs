using AO.Models.Interfaces;
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
    public class DbUserClaimsFactory<TUser> : UserClaimsPrincipalFactory<IdentityUser> where TUser : IUserBase, new()
    {
        public DbUserClaimsFactory(
            UserManager<IdentityUser> userManager,
            IOptions<IdentityOptions> optionsAccessor,
            DbUserClaimsConverter<TUser> claimsConverter) : base(userManager, optionsAccessor)
        {
            ClaimsConverter = claimsConverter;
        }

        public DbUserClaimsConverter<TUser> ClaimsConverter { get; }
        
        protected async override Task<ClaimsIdentity> GenerateClaimsAsync(IdentityUser user)
        {            
            var result = await base.GenerateClaimsAsync(user);
            var dbUser = await ClaimsConverter.QueryUserAsync(user.UserName);
            var claims = ClaimsConverter.GetClaimsFromUser(dbUser);
            result.AddClaims(claims);
            return result;
        }
    }
}
