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
                "dbo.SomethingElse", "DROP TABLE %obj%",
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

    }
}
