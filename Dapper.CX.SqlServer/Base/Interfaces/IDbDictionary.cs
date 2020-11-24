using System.Threading.Tasks;

namespace Dapper.CX.Abstract
{
    public interface IDbDictionary<TKey>
    {
        string TableName { get; }

        Task DeleteAsync(TKey key);
        Task<TValue> GetAsync<TValue>(TKey key, TValue defaultValue = default);
        Task InitializeAsync();
        Task<bool> KeyExistsAsync(TKey key);
        Task SetAsync<TValue>(TKey key, TValue value);
    }
}