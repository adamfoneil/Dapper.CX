using Dapper.CX.Abstract;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Dapper.CX.SqlServer.Services
{
    public class SqlServerLongCrudService : SqlCrudService<long>
    {
        private readonly string _connectionString;

        public SqlServerLongCrudService(string connectionString) : base(new SqlServerLongCrudProvider())
        {
            _connectionString = connectionString;
        }

        public override IDbConnection GetConnection()
        {
            return new SqlConnection(_connectionString);
        }
    }
}
