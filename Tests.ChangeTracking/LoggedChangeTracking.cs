using Dapper;
using Dapper.CX.Classes;
using Dapper.CX.SqlServer;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlServer.LocalDb;
using System;
using Tests.Models;

namespace Tests.Base
{
    [TestClass]
    public class LoggedChangeTracking
    {
        private const string dbName = "DapperCX";

        [ClassInitialize]
        public static void Initialize(TestContext context)
        {
            using (var cn = LocalDb.GetConnection(dbName))
            {
                LocalDb.ExecuteInitializeStatements(cn, DbObjects.CreateObjects());
            }
        }

        [TestMethod]
        public void CaptureChanges()
        {
            using (var cn = LocalDb.GetConnection(dbName))
            {
                var emp = new Employee()
                {
                    LastName = "Herbert",
                    FirstName = "Yavad",
                    HireDate = new DateTime(1990, 1, 1),
                    IsExempt = true
                };

                // need to make sure record doesn't exist
                cn.Execute("DELETE [dbo].[Employee] WHERE [FirstName]=@firstName AND [LastName]=@lastName", emp);

                var provider = new SqlServerIntCrudProvider();
                provider.SaveAsync(cn, emp).Wait();

                var ct = new LoggedChangeTracker<Employee>("adamo", emp);

                emp.FirstName = "Javad";
                emp.Status = Status.Inactive;
                provider.SaveAsync(cn, emp, ct).Wait();
            }
        }
    }
}
