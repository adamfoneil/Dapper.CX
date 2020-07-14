using AO.Models.Interfaces;
using Dapper.CX.Abstract;
using Microsoft.Data.SqlClient;
using System;
using System.Data;

namespace Dapper.CX.SqlServer.Services
{
    public partial class SqlServerCrudService<TIdentity, TUser> : SqlCrudService<TIdentity, TUser> where TUser : IUserBase
    {
        public SqlServerCrudService(string connectionString, string userName, Func<object, TIdentity> convertIdentity) : base(connectionString, userName, new SqlServerCrudProvider<TIdentity>(convertIdentity))
        {
        }

        public override IDbConnection GetConnection() => new SqlConnection(_connectionString);
    }
}
