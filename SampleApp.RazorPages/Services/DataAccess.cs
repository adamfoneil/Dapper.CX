using Dapper.CX.SqlServer.Services;
using SampleApp.Models;
using System;

namespace SampleApp.RazorPages.Services
{
    public class DataAccess : SqlServerCrudService<int, UserProfile>
    {
        public DataAccess(
            string connectionString, UserProfile user, Func<object, int> convertIdentity) : base(connectionString, user, convertIdentity)
        {
        }
    }
}
