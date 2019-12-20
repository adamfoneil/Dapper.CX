using Dapper.CX.Abstract;
using Dapper.CX.SqlServer;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlServer.LocalDb;
using SqlServer.LocalDb.Models;
using System.Collections.Generic;
using System.Data;

namespace Tests.SqlServer
{
    [TestClass]
    public class SqlServerIntegrationInt : IntegrationBase<int>
    {
        protected override IDbConnection GetConnection()
        {
            return LocalDb.GetConnection("DapperCX", CreateObjects());
        }

        private IEnumerable<InitializeStatement> CreateObjects()
        {
            yield return new InitializeStatement(
                "dbo.Employee",
                "DROP TABLE %obj%",
                @"CREATE TABLE %obj% (
                    [FirstName] nvarchar(50) NOT NULL,
                    [LastName] nvarchar(50) NOT NULL,
                    [HireDate] date NULL,
                    [TermDate] date NULL,
                    [IsExempt] bit NOT NULL,
                    [Id] int identity(1, 1) PRIMARY KEY
                )");
        }

        protected override SqlCrudProvider<int> GetProvider()
        {
            return new SqlServerIntCrudProvider();
        }

        [ClassInitialize]
        public static void Initialize(TestContext context)
        {
            LocalDb.TryDropDatabase("DapperCX", out _);
        }

        [TestMethod]
        public void NewObjShouldBeNew()
        {
            NewObjShouldBeNewBase();
        }

        [TestMethod]
        public void Insert()
        {
            InsertBase();
        }

        [TestMethod]
        public void Update()
        {
            UpdateBase();
        }
    }
}
