using Dapper;
using Dapper.CX.Classes;
using Dapper.CX.Extensions;
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
                try { cn.Execute("TRUNCATE TABLE [changes].[RowVersion]"); } catch {  /*do nothing */ }
                try { cn.Execute("TRUNCATE TABLE [changes].[ColumnHistory]"); } catch { /* do nothing */ }
                cn.Execute("DELETE [dbo].[Employee] WHERE [FirstName]=@firstName AND [LastName]=@lastName", emp);

                var provider = new SqlServerIntCrudProvider();
                provider.SaveAsync(cn, emp).Wait();

                var ct = new LoggedChangeTracker<Employee>("adamo", emp);

                emp.FirstName = "Javad";
                emp.Status = Status.Inactive;
                provider.SaveAsync(cn, emp, ct).Wait();

                Assert.IsTrue(cn.RowExistsAsync("[changes].[RowVersion] WHERE [TableName]='Employee' AND [RowId]=1 AND [Version]=1").Result);
                Assert.IsTrue(cn.RowExistsAsync("[changes].[ColumnHistory] WHERE [TableName]='Employee' AND [RowId]=1 AND [Version]=1 AND [ColumnName]='FirstName' AND [OldValue]='Yavad' AND [NewValue]='Javad'").Result);
                Assert.IsTrue(cn.RowExistsAsync("[changes].[ColumnHistory] WHERE [TableName]='Employee' AND [RowId]=1 AND [Version]=1 AND [ColumnName]='Status' AND [OldValue]='Active' AND [NewValue]='Inactive'").Result);
            }
        }
    }
}
