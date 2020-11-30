using Dapper;
using Dapper.CX.SqlServer;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlServer.LocalDb;
using System;
using System.Data;
using System.Linq;
using Tests.Models;

namespace Tests.Base
{
    [TestClass]
    public class SqlIntegration
    {
        private SqlServerCrudProvider<int> GetProvider() => new SqlServerCrudProvider<int>(id => Convert.ToInt32(id));

        [ClassInitialize]
        public static void Initialize(TestContext context)
        {
            LocalDb.TryDropDatabase("DapperCX", out _);

            using (var cn = LocalDb.GetConnection("DapperCX"))
            {
                LocalDb.ExecuteInitializeStatements(cn, DbObjects.CreateObjects());
            }
        }

        private IDbConnection GetConnection()
        {
            return LocalDb.GetConnection("DapperCX");
        }        

        [TestMethod]
        public void UpdateWithLambda()
        {
            var emp = new Employee() { FirstName = "Janzy", LastName = "Horzenyadle", Id = 249578 };
            var provider = GetProvider();

            using (var cn = GetConnection())
            {
                provider.UpdateAsync(cn, emp, m => m.FirstName, m => m.LastName).Wait();
            }
        }        

        [TestMethod]
        public void PlainInsertAsync()
        {
            var emp = new Employee() { FirstName = "Janzy", LastName = "Horzenyadle" };
            var provider = GetProvider();

            using (var cn = GetConnection())
            {                
                var result = provider.InsertAsync(cn, emp, getIdentity: false).Result;
                Assert.IsTrue(result == default);
            }
        }

        [TestMethod]
        public void PlainInsert()
        {
            var emp = new Employee() { FirstName = "Janzy", LastName = "Horzenyadle" };
            var provider = GetProvider();

            using (var cn = GetConnection())
            {                
                var result = provider.Insert(cn, emp, getIdentity: false);
                Assert.IsTrue(result == default);
            }
        }

        [TestMethod]
        public void DeleteByModel()
        {
            var dummyValue = new string[] { "this", "that" };
            var emp = new Employee() 
            { 
                FirstName = "Loozy", 
                LastName = "Vorschindle",
                Something = dummyValue
            };
            var provider = GetProvider();

            using (var cn = GetConnection())
            {
                var result = provider.Insert(cn, emp);
                provider.DeleteAsync(cn, emp).Wait();
                Assert.IsTrue(emp.Something.SequenceEqual(dummyValue));
                Assert.IsTrue(!provider.ExistsAsync<Employee>(cn, emp.Id).Result);
                Assert.IsTrue(emp.Value == OtherEnum.Other); // set by ITrigger
            }
        }

        [TestMethod]
        public void DeleteById()
        {
            using (var cn = GetConnection())
            {
                var emp = new Employee()
                {
                    FirstName = "Loozy",
                    LastName = "Vorschindle"                    
                };

                var provider = GetProvider();
                var id = provider.InsertAsync(cn, emp).Result;

                provider.DeleteAsync<Employee>(cn, id).Wait();

                emp = provider.GetAsync<Employee>(cn, id).Result;
                Assert.IsNull(emp);
            }

        }

        [TestMethod]
        public void DbDictionaryCoreOperations()
        {
            var sampleDictionary = new SessionDbDictionary(GetConnection);
            var input = new Employee()
            {
                FirstName = "whoever",
                LastName = "nobody",
                Status = Status.Active
            };

            sampleDictionary.SetAsync("hello", input).Wait();

            var entry = sampleDictionary.GetAsync<Employee>("hello").Result;
            Assert.IsTrue(input.FirstName.Equals("whoever"));
            Assert.IsTrue(input.LastName.Equals("nobody"));
            Assert.IsTrue(input.Status == Status.Active);

            entry.FirstName = "Django";
            sampleDictionary.SetAsync("hello", entry).Wait();
            entry = sampleDictionary.GetAsync<Employee>("hello").Result;
            Assert.IsTrue(entry.FirstName.Equals("Django"));

            Assert.IsTrue(sampleDictionary.KeyExistsAsync("hello").Result);

            sampleDictionary.DeleteAsync("hello").Wait();

            using (var cn = GetConnection())
            {
                cn.Execute("DROP TABLE [session].[User]");
                cn.Execute("DROP SCHEMA [session]");
            }
        }        
    }
}
