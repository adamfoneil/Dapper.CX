using Dapper.CX.SqlServer;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tests.Extensions;
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

        [TestMethod]
        public void InsertStatementBase()
        {
            string sql = new SqlServerIntCrudProvider().GetInsertStatement(typeof(Employee));
            const string result =
                @"INSERT INTO [Employee] (
                    [FirstName], [LastName], [HireDate], [TermDate], [IsExempt], [Timestamp], [Status], [Value]
                ) VALUES (
                    @FirstName, @LastName, @HireDate, @TermDate, @IsExempt, @Timestamp, @Status, @Value
                ); SELECT SCOPE_IDENTITY();";

            Assert.IsTrue(sql.ReplaceWhitespace().Equals(result.ReplaceWhitespace()));
        }

        [TestMethod]
        public void UpdateStatementBase()
        {
            string sql = new SqlServerIntCrudProvider().GetUpdateStatement<Employee>();
            const string result =
                @"UPDATE [Employee] SET 
                    [FirstName]=@FirstName, [LastName]=@LastName, [HireDate]=@HireDate, [TermDate]=@TermDate, [IsExempt]=@IsExempt, [Timestamp]=@Timestamp, [Status]=@Status, [Value]=@Value
                WHERE
                    [Id]=@Id";

            Assert.IsTrue(sql.ReplaceWhitespace().Equals(result.ReplaceWhitespace()));
        }

    }
}
