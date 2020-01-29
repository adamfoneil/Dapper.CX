using Dapper.CX.Abstract;
using Dapper.CX.Classes;
using Dapper.CX.SqlServer;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Tests.Extensions;
using Tests.Models;

namespace Tests.SqlServer
{
    [TestClass]
    public class SqlServerStatements
    {
        protected SqlCrudProvider<int> GetProvider() => new SqlServerIntCrudProvider();

        [TestMethod]
        public void InsertStatement()
        {
            string sql = GetProvider().GetInsertStatement(typeof(Employee));
            const string result = 
                @"INSERT INTO [Employee] (
                    [FirstName], [LastName], [HireDate], [TermDate], [IsExempt], [Timestamp]
                ) VALUES (
                    @FirstName, @LastName, @HireDate, @TermDate, @IsExempt, @Timestamp
                ); SELECT SCOPE_IDENTITY();";
            
            Assert.IsTrue(sql.ReplaceWhitespace().Equals(result.ReplaceWhitespace()));
        }

        [TestMethod]
        public void UpdateStatement()
        {
            string sql = GetProvider().GetUpdateStatement(new Employee());
            const string result = 
                @"UPDATE [Employee] SET 
                    [FirstName]=@FirstName, [LastName]=@LastName, [HireDate]=@HireDate, [TermDate]=@TermDate, [IsExempt]=@IsExempt, [Timestamp]=@Timestamp
                WHERE
                    [Id]=@Id";
            
            Assert.IsTrue(sql.ReplaceWhitespace().Equals(result.ReplaceWhitespace()));
        }

        [TestMethod]
        public void UpdateStatementModifiedColumns()
        {
            var emp = new Employee()
            {
                Id = 234,
                FirstName = "chonga",
                LastName = "wimbus",
                HireDate = new DateTime(1990, 1, 1)
            };

            var ct = new ChangeTracker<Employee>(emp);

            emp.FirstName = "argo";

            string sql = GetProvider().GetUpdateStatement(emp, ct);
            const string result =
                @"UPDATE [Employee] SET 
                    [FirstName]=@FirstName 
                WHERE
                    [Id]=@Id";
            
            Assert.IsTrue(sql.ReplaceWhitespace().Equals(result.ReplaceWhitespace()));
        }        

        [TestMethod]
        public void DeleteStatement()
        {
            string sql = GetProvider().GetDeleteStatement(typeof(Employee));
            Assert.IsTrue(sql.Equals("DELETE [Employee] WHERE [Id]=@id"));
        }

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
