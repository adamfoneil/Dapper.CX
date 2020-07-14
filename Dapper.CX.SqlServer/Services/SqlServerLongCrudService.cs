using AO.Models.Interfaces;
using Dapper.CX.Abstract;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Dapper.CX.SqlServer.Services
{
    public class SqlServerLongCrudService<TUser> : SqlCrudService<long, TUser> where TUser : IUserBase
    {
        public SqlServerLongCrudService(string connectionString, string userName) : base(connectionString, new SqlServerLongCrudProvider(), userName)
        {                        
        }

        public override IDbConnection GetConnection() => new SqlConnection(_connectionString);
    }
}
