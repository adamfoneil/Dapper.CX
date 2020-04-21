using System.Data;
using System.Threading.Tasks;

namespace Dapper.CX.Interfaces
{
    /// <summary>
    /// use this to make anything able to be saved in a database.
    /// Intended for <see cref="Dapper.CX.Classes.ChangeTracker{TModel}"/> but actually usable for anything
    /// </summary>
    public interface IDbSaveable
    {
        Task SaveAsync(IDbConnection connection);
    }
}
