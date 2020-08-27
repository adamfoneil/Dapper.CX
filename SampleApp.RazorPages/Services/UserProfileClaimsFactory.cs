using Dapper.CX.SqlServer.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using SampleApp.Models;

namespace SampleApp.RazorPages.Services
{
    public class UserProfileClaimsFactory : DbUserClaimsFactory<UserProfile>
    {        
        public UserProfileClaimsFactory(
            DbUserClaimsConverter<UserProfile> claimConverter,
            UserManager<IdentityUser> userManager,
            IOptions<IdentityOptions> optionsAccessor) : base(userManager, optionsAccessor, claimConverter)
        {     
        }
    }
}
