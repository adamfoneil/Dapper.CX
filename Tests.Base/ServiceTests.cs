using Dapper.CX.Abstract;
using Dapper.CX.SqlServer;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlServer.LocalDb;
using System.Data;

namespace Tests.Base
{
    [TestClass]
    public class ServiceTests
    {
        [TestMethod]
        public void GetEmployeeWithTxn()
        {

        }
    }

    public class TestService : SqlCrudService<int>
    {
        public TestService() : base(new SqlServerIntCrudProvider())
        {
        }

        public override IDbConnection GetConnection() => LocalDb.GetConnection("DapperCX");
    }
}
