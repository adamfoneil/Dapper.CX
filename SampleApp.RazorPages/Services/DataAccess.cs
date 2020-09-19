using Dapper.CX.SqlServer.Services;
using SampleApp.Models;
using System;

namespace SampleApp.RazorPages.Services
{
    /// <summary>
    /// sample derived crud service class for use with custom serviceFactory AddDapperCX overload
    /// </summary>
    public class DataAccess : SqlServerCrudService<int, UserProfile>
    {
        public DataAccess(string connectionString, UserProfile user, Func<object, int> convertIdentity) : base(connectionString, user, convertIdentity)
        {
        }
    }
}
