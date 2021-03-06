using Dapper.CX.SqlServer;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Tests.Extensions;
using Tests.Models;

namespace Tests.Base
{
    [TestClass]
    public class SqlStatements
    {
        private SqlServerCrudProvider<int> GetProvider() => new SqlServerCrudProvider<int>(id => Convert.ToInt32(id));

        [TestMethod]
        public void InsertWithColumns()
        {
            var provider = GetProvider();
            string result = provider.GetInsertStatement(typeof(Employee), new string[] { "FirstName", "LastName" });
            Assert.IsTrue(result.ReplaceWhitespace().Equals(
                @"INSERT INTO [Employee] (
                    [FirstName], [LastName]
                ) VALUES (
                    @FirstName, @LastName
                ); SELECT SCOPE_IDENTITY();".ReplaceWhitespace()));
        }

        [TestMethod]
        public void UpdateWithColumns()
        {
            var provider = GetProvider();
            string result = provider.GetUpdateStatement<Employee>(columnNames: new string[] { "FirstName", "LastName" });
            Assert.IsTrue(result.ReplaceWhitespace().Equals(
                @"UPDATE [Employee] SET 
                    [FirstName]=@FirstName, [LastName]=@LastName 
                WHERE 
                    [Id]=@Id".ReplaceWhitespace()));
        }

        [TestMethod]
        public void InsertStatementBase()
        {
            string sql = GetProvider().GetInsertStatement(typeof(Employee));
            const string result =
                @"INSERT INTO [Employee] (
                    [FirstName], [LastName], [HireDate], [TermDate], [IsExempt], [Timestamp], [Status], [Value], [Comments]
                ) VALUES (
                    @FirstName, @LastName, @HireDate, @TermDate, @IsExempt, @Timestamp, @Status, @Value, @Comments
                ); SELECT SCOPE_IDENTITY();";

            Assert.IsTrue(sql.ReplaceWhitespace().Equals(result.ReplaceWhitespace()));
        }

        [TestMethod]
        public void UpdateStatementBase()
        {
            string sql = GetProvider().GetUpdateStatement<Employee>();
            const string result =
                @"UPDATE [Employee] SET 
                    [FirstName]=@FirstName, [LastName]=@LastName, [HireDate]=@HireDate, [TermDate]=@TermDate, [IsExempt]=@IsExempt, [Timestamp]=@Timestamp, [Status]=@Status, [Value]=@Value, [Comments]=@Comments
                WHERE
                    [Id]=@Id";

            Assert.IsTrue(sql.ReplaceWhitespace().Equals(result.ReplaceWhitespace()));
        }

    }
}
