using Dapper.CX.Abstract;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Dapper.CX.SqlServer.Services
{
    public class SqlServerLongCrudService : SqlCrudService<long>
    {
        public SqlServerLongCrudService(string connectionString) : base(connectionString, new SqlServerLongCrudProvider())
        {
        }

        public override IDbConnection GetConnection()
        {
            return new SqlConnection(_connectionString);
        }
    }
}
