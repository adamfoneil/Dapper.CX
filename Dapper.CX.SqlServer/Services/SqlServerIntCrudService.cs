using Dapper.CX.Abstract;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Dapper.CX.SqlServer.Services
{
    public class SqlServerIntCrudService : SqlCrudService<int>
    {
        private readonly string _connectionString;

        public SqlServerIntCrudService(string connectionString) : base(new SqlServerIntCrudProvider())
        {
            _connectionString = connectionString;
        }

        public override IDbConnection GetConnection()
        {
            return new SqlConnection(_connectionString);
        }
    }
}
