using AO.Models.Interfaces;

namespace Dapper.CX.Interfaces
{
    public interface IGetUser<T> where T : IUserBase
    {
        /// <summary>
        /// Request-scope access to user.
        /// Synchronous because it's used during Startup.ConfigureServices which is not async
        /// </summary>
        T Get(string userName);
    }
}
