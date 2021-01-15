using AO.Models.Interfaces;
using System.Collections.Generic;
using System.Security.Claims;

namespace Dapper.CX.Interfaces
{
    public interface IOnboardUser<T> where T : IUserBase
    {
        /// <summary>
        /// Request-scope access to user.
        /// Synchronous because it's used during Startup.ConfigureServices which is not async
        /// </summary>
        T Get(string userName, IEnumerable<Claim> claims);
    }
}
