using Dapper.CX.Abstract;
using Dapper.CX.Classes;
using Dapper.CX.SqlServer;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics;
using Tests.Models;

namespace Tests.SqlServer
{
    [TestClass]
    public class SqlServerStatements
    {
        protected SqlCrudProvider<int> GetProvider() => new SqlServerIntCrudProvider();

        [TestMethod]
        public void InsertStatementBase()
        {
            string sql = GetProvider().GetInsertStatement(typeof(Employee));
            const string result = 
                @"INSERT INTO [Employee] (
                    [FirstName], [LastName], [HireDate], [TermDate], [IsExempt]
                ) VALUES (
                    @FirstName, @LastName, @HireDate, @TermDate, @IsExempt
                ); SELECT SCOPE_IDENTITY();";

            PrintDiffInfo(sql, result);
            Assert.IsTrue(sql.Equals(result));
        }

        [TestMethod]
        public void UpdateStatementBase()
        {
            string sql = GetProvider().GetUpdateStatement(new Employee());
            const string result = 
                @"UPDATE [Employee] SET 
                    [FirstName]=@FirstName, [LastName]=@LastName, [HireDate]=@HireDate, [TermDate]=@TermDate, [IsExempt]=@IsExempt 
                WHERE 
                    [Id]=@Id";

            PrintDiffInfo(sql, result);
            Assert.IsTrue(sql.Equals(result));
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

            PrintDiffInfo(sql, result);
            Assert.IsTrue(sql.Equals(result));
        }

        private void PrintDiffInfo(string string1, string string2)
        {
            if (!string1.Equals(string2))
            {

            }
        }
    }
}
