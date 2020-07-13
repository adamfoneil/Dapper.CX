using AO.Models.Interfaces;
using Dapper.CX.Abstract;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Threading.Tasks;

namespace Dapper.CX.SqlServer.Services
{
    public class SqlServerIntCrudService<TUser> : SqlCrudService<int, TUser> where TUser : IUserBase
    {
        private readonly string _connectionString;        

        public SqlServerIntCrudService(string connectionString, string userName) : base(new SqlServerIntCrudProvider(), userName)
        {
            _connectionString = connectionString;            
        }

        public override IDbConnection GetConnection() => new SqlConnection(_connectionString);

        protected override TUser QueryUser(IDbConnection connection, string userName)
        {
            throw new System.NotImplementedException();
        }

        protected override Task UpdateUserInnerAsync(IDbConnection connection, TUser user)
        {
            throw new System.NotImplementedException();
        }
    }
}
