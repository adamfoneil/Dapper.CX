using Dapper.CX.Interfaces;
using System;
using System.Data;
using System.Threading.Tasks;

namespace Dapper.CX.Classes
{
    public class LoggedChangeTracker<TModel> : ChangeTracker<TModel>, IDbSaveable
    {
        public LoggedChangeTracker(TModel @object) : base(@object)
        {
        }

        public Task SaveAsync(IDbConnection connection)
        {
            throw new NotImplementedException();
        }
    }
}
