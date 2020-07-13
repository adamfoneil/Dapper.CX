using AO.Models.Interfaces;
using Dapper.CX.Abstract;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Dapper.CX.SqlServer.Services
{
    public abstract class SqlServerLongCrudService<TUser> : SqlCrudService<long, TUser> where TUser : IUserBase
    {
        private readonly string _connectionString;        

        public SqlServerLongCrudService(string connectionString, string userName) : base(new SqlServerLongCrudProvider(), userName)
        {
            _connectionString = connectionString;            
        }

        public override IDbConnection GetConnection()
        {
            return new SqlConnection(_connectionString);
        }
    }
}
