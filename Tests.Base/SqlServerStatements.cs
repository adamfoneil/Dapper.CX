using Dapper.CX.Abstract;
using Dapper.CX.SqlServer;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlServer.LocalDb;
using SqlServer.LocalDb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using Tests.Models;

namespace Tests.Base
{
    [TestClass]
    public class SqlServerStatements
    {
        protected SqlCrudProvider<int> GetProvider() => new SqlServerIntCrudProvider();

        private static IEnumerable<InitializeStatement> AdditionalObjects()
        {
            yield return new InitializeStatement(
                "dbo.SomethingElse", "DROP %obj%",
                @"CREATE TABLE %obj% (
                    [EmployeeId] int NOT NULL,
                    [Balance] decimal NULL,
                    [Whatever] nvarchar(50) NULL,
                    [Id] int identity(1,1) PRIMARY KEY
                )");
        }

        [ClassInitialize]
        public static void Initialize(TestContext context)
        {
            LocalDb.TryDropDatabase("DapperCX", out _);

            using (var cn = LocalDb.GetConnection("DapperCX"))
            {
                LocalDb.ExecuteInitializeStatements(cn, DbObjects.CreateObjects().Concat(AdditionalObjects()));
            }
        }

        [TestMethod]
        public void CustomGetStatement()
        {
            string sql = GetProvider().GetQuerySingleStatement(typeof(EmployeeCustom));
            Assert.IsTrue(sql.Equals(@"SELECT [emp].*, [se].[Balance], [se].[Whatever]
            FROM [dbo].[Employee] [emp]
            LEFT JOIN [dbo].[SomethingElse] [se] ON [emp].[Id]=[se].[EmployeeId] WHERE [emp].[Id]=@id"));
        }

        [TestMethod]
        public void GetEmployeeWithRelated()
        {
            using (var cn = LocalDb.GetConnection("DapperCX"))
            {
                var emp = new EmployeeCustom()
                {
                    FirstName = "Whoever",
                    LastName = "Nobody",
                    IsExempt = true,
                    HireDate = DateTime.Today
                };

                var provider = GetProvider();
                int id = provider.SaveAsync(cn, emp).Result;

                emp = provider.GetAsync<EmployeeCustom>(cn, id).Result;
                Assert.IsTrue(emp.Something.SequenceEqual(new string[] { "this", "that", "other" }));
                Assert.IsTrue(emp.SomethingElse.SequenceEqual(new DateTime[] { DateTime.Today }));
            }
        }
    }
}
