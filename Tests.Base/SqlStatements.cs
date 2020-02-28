using Dapper.CX.SqlServer;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tests.Models;

namespace Tests.Base
{
    [TestClass]
    public class SqlStatements
    {
        [TestMethod]
        public void InsertWithColumns()
        {
            var provider = new SqlServerIntCrudProvider();
            string result = provider.GetInsertStatement(typeof(Employee), new string[] { "FirstName", "LastName" });
            Assert.IsTrue(result.Equals(
                @"INSERT INTO [Employee] (
                    [FirstName], [LastName]
                ) VALUES (
                    @FirstName, @LastName
                ); SELECT SCOPE_IDENTITY();"));
        }

        [TestMethod]
        public void UpdateWithColumns()
        {
            var provider = new SqlServerIntCrudProvider();
            string result = provider.GetUpdateStatement<Employee>(columnNames: new string[] { "FirstName", "LastName" });
            Assert.IsTrue(result.Equals(
                @"UPDATE [Employee] SET 
                    [FirstName]=@FirstName, [LastName]=@LastName 
                WHERE 
                    [Id]=@Id"));
        }
    }    
}
