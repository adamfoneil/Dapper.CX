using AO.Models.Interfaces;
using Dapper.CX.Abstract;
using Dapper.CX.Classes;
using Microsoft.Data.SqlClient;
using System;
using System.Data;

namespace Dapper.CX.SqlServer.Services
{
    public partial class DapperCX<TIdentity, TUser> : SqlCrudService<TIdentity, TUser> where TUser : IUserBase
    {
        public DapperCX(string connectionString, TUser user, Func<object, TIdentity> convertIdentity) : base(connectionString, user, new SqlServerCrudProvider<TIdentity>(convertIdentity))
        {
        }

        public DapperCX(string connectionString, TUser user, SqlServerCrudProvider<TIdentity> crudProvider) : base(connectionString, user, crudProvider)
        {
        }

        public override IDbConnection GetConnection() => new SqlConnection(_connectionString);
    }

    public partial class DapperCX<TIdentity> : DapperCX<TIdentity, SystemUser>
    {
        public DapperCX(string connectionString, Func<object, TIdentity> convertIdentity) : base(connectionString, new SystemUser("system"), convertIdentity)
        {
        }
    }
}
