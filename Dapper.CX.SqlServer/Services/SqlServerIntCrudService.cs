using AO.Models.Interfaces;
using Dapper.CX.Abstract;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Dapper.CX.SqlServer.Services
{
    public class SqlServerIntCrudService<TUser> : SqlCrudService<int, TUser> where TUser : IUserBase
    {
        public SqlServerIntCrudService(string connectionString, string userName) : base(connectionString, new SqlServerIntCrudProvider(), userName)
        {
        }

        public override IDbConnection GetConnection() => new SqlConnection(_connectionString);        
    }
}
