using Dapper.CX.Abstract;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Dapper.CX.SqlServer.Services
{
    public class SqlServerIntCrudService : SqlCrudService<int>
    {
        public SqlServerIntCrudService(string connectionString) : base(connectionString, new SqlServerIntCrudProvider())
        {
        }

        public override IDbConnection GetConnection()
        {
            return new SqlConnection(_connectionString);
        }
    }
}
