using Dapper;
using Dapper.CX.Abstract;
using Dapper.CX.Classes;
using Dapper.CX.Extensions;
using Dapper.CX.SqlServer;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlServer.LocalDb;
using System;
using System.Data;
using System.Linq;
using Tests.Models;

namespace Tests.SqlServer
{
    [TestClass]
    public class SqlServerIntegrationInt : IntegrationBase<int>
    {
        [ClassInitialize]
        public static void Initialize(TestContext context)
        {
            LocalDb.TryDropDatabase("DapperCX", out _);

            using (var cn = LocalDb.GetConnection("DapperCX"))
            {
                LocalDb.ExecuteInitializeStatements(cn, DbObjects.CreateObjects());
            }            
        }

        protected override IDbConnection GetConnection()
        {
            return LocalDb.GetConnection("DapperCX");
        }

        protected override SqlCrudProvider<int> GetProvider()
        {
            return new SqlServerCrudProvider<int>(id => Convert.ToInt32(id));            
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

        [TestMethod]
        public void Delete()
        {
            DeleteBase();
        }

        [TestMethod]
        public void Exists()
        {
            ExistsBase();
        }

        [TestMethod]
        public void ExistsWhere()
        {
            ExistsWhereBase();
        }

        [TestMethod]
        public void MergeExplicitProps()
        {
            MergeExplicitPropsBase();
        }

        [TestMethod]
        public void MergePKProps()
        {
            MergePKPropsBase();
        }

        [TestMethod]
        public void CmdDictionaryInsert()
        {            
            using (var cn = GetConnection())
            {
                var cmd = SqlServerCmd.FromTableSchemaAsync(cn, "dbo", "Employee").Result;
                cmd["FirstName"] = "Wilbur";
                cmd["LastName"] = "Wainright";
                cmd["IsExempt"] = true;
                cmd["Timestamp"] = new SqlExpression("getdate()");
                cmd["Status"] = Status.Active;

                var sql = cmd.GetInsertStatement();
                var id = cmd.InsertAsync<int>(cn).Result;
                Assert.IsTrue(cn.RowExistsAsync("[dbo].[Employee] WHERE [LastName]='Wainright'").Result);
            }            
        }        

        [TestMethod]
        public void CmdDictionaryUpdate()
        {
            // create our sample row
            CmdDictionaryInsert();

            using (var cn = GetConnection())
            {
                var cmd = SqlServerCmd.FromTableSchemaAsync(cn, "dbo", "Employee").Result;
                cmd["FirstName"] = "Wilbur";
                cmd["LastName"] = "Wainright2";
                cmd["IsExempt"] = true;
                cmd["Timestamp"] = new SqlExpression("getdate()");
                cmd["Status"] = Status.Inactive;
                cmd["Value"] = OtherEnum.That;
                
                cmd.UpdateAsync(cn, 1).Wait();
                Assert.IsTrue(cn.RowExistsAsync("[dbo].[Employee] WHERE [LastName]='Wainright2'").Result);
            }
        }

        [TestMethod]
        public void SqlServerCmdFromQuery()
        {
            CmdDictionaryInsert();

            using (var cn = GetConnection())
            {
                int id = cn.Query<int>("SELECT [Id] FROM [dbo].[Employee]").First();
                var cmd = SqlServerCmd.FromQueryAsync(cn, "SELECT * FROM [dbo].[Employee] WHERE [Id]=@id", new { id }, "Id").Result;
                var columns = cmd.Select(kp => kp.Key).ToArray();
                Assert.IsTrue(columns.SequenceEqual(new string[] { "FirstName", "LastName", "HireDate", "TermDate", "IsExempt", "Timestamp", "Status", "Value" }));
            }
        }

        [TestMethod]
        public void SqlServerCmdFromQueryWithId()
        {
            CmdDictionaryInsert();

            using (var cn = GetConnection())
            {
                int id = cn.Query<int>("SELECT [Id] FROM [dbo].[Employee]").First();
                var cmd = SqlServerCmd.FromQueryAsync(cn, "SELECT * FROM [dbo].[Employee] WHERE [Id]=@id", new { id }).Result;
                var columns = cmd.Select(kp => kp.Key).ToArray();
                Assert.IsTrue(columns.SequenceEqual(new string[] { "FirstName", "LastName", "HireDate", "TermDate", "IsExempt", "Timestamp", "Status", "Value", "Id" }));
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
                    HireDate = DateTime.Today,
                    Value = OtherEnum.Other
                };

                var provider = GetProvider();
                int id = provider.SaveAsync(cn, emp).Result;

                emp = provider.GetAsync<EmployeeCustom>(cn, id).Result;
                Assert.IsTrue(emp.Something.SequenceEqual(new string[] { "this", "that", "other" }));
                Assert.IsTrue(emp.SomethingElse.SequenceEqual(new DateTime[] { DateTime.Today }));
            }
        }

        [TestMethod]
        public void TransactionRollback()
        {
            var provider = GetProvider();

            const string lastName = "Thorgas";
            const string firstName = "Enslow";

            using (var cn = GetConnection())
            {                
                using (var txn = cn.BeginTransaction())
                {
                    provider.SaveAsync(cn, new Employee()
                    {
                        FirstName = firstName,
                        LastName = lastName
                    }, txn: txn).Wait();

                    txn.Rollback();
                }

                Assert.IsTrue(!provider.ExistsWhereAsync<Employee>(cn, new { lastName, firstName }).Result);
            }
        }

        [TestMethod]
        public void CmdDictionaryInsertGuid()
        {
            const string sampleRole = "Sample Role";

            using (var cn = GetConnection())
            {
                cn.Execute("DELETE [dbo].[AspNetRoles] WHERE [Name]=@sampleRole", new { sampleRole });

                new SqlServerCmd("dbo.AspNetRoles")
                {
                    ["Name"] = sampleRole,
                    ["NormalizedName"] = sampleRole,
                    ["Id"] = Guid.NewGuid()
                }.InsertAsync(cn).Wait();
            }
        }

    } 
}
