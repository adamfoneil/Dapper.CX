using Dapper.CX.Abstract;
using Dapper.CX.SqlServer;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tests.Models;

namespace Tests.Base
{
    [TestClass]
    public class SqlServerStatements
    {
        protected SqlCrudProvider<int> GetProvider() => new SqlServerIntCrudProvider();

        [TestMethod]
        public void CustomGetStatement()
        {
            string sql = GetProvider().GetQuerySingleStatement(typeof(EmployeeCustom));
            Assert.IsTrue(sql.Equals(@"SELECT [emp].*, [se].[Balance], [se].[Whatever]
            FROM [dbo].[Employee] [emp]
            LEFT JOIN [dbo].[SomethingElse] [se] ON [emp].[Id]=[se].[EmployeeId] WHERE [emp].[Id]=@id"));
        }
    }
}
