using Dapper.CX.SqlServer;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlServer.LocalDb;
using System.Data;
using Tests.Models;

namespace Tests.Base
{
    [TestClass]
    public class SqlIntegration
    {
        [ClassInitialize]
        public static void Initialize(TestContext context)
        {
            LocalDb.TryDropDatabase("DapperCX", out _);

            using (var cn = LocalDb.GetConnection("DapperCX"))
            {
                LocalDb.ExecuteInitializeStatements(cn, DbObjects.CreateObjects());
            }
        }

        private IDbConnection GetConnection()
        {
            return LocalDb.GetConnection("DapperCX");
        }        

        [TestMethod]
        public void UpdateWithLambda()
        {
            var emp = new Employee() { FirstName = "Janzy", LastName = "Horzenyadle", Id = 249578 };
            var provider = new SqlServerIntCrudProvider();

            using (var cn = GetConnection())
            {
                provider.UpdateAsync(cn, emp, m => m.FirstName, m => m.LastName).Wait();
            }
        }        

        [TestMethod]
        public void PlainInsertAsync()
        {
            var emp = new Employee() { FirstName = "Janzy", LastName = "Horzenyadle" };
            var provider = new SqlServerIntCrudProvider();

            using (var cn = GetConnection())
            {                
                var result = provider.InsertAsync(cn, emp, getIdentity: false).Result;
                Assert.IsTrue(result == default);
            }
        }

        [TestMethod]
        public void PlainInsert()
        {
            var emp = new Employee() { FirstName = "Janzy", LastName = "Horzenyadle" };
            var provider = new SqlServerIntCrudProvider();

            using (var cn = GetConnection())
            {                
                var result = provider.Insert(cn, emp, getIdentity: false);
                Assert.IsTrue(result == default);
            }
        }
    }
}
