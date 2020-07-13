using AO.Models.Interfaces;
using Dapper.CX.Abstract;
using Dapper.CX.SqlServer;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlServer.LocalDb;
using System;
using System.Data;
using Tests.Models;

namespace Tests.Base
{
    [TestClass]
    public class ServiceTests
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

        [TestMethod]
        public void EmployeeBasicActions()
        {
            var service = new TestService();
            var id = service.SaveAsync(new Employee()
            {
                FirstName = "Yardie",
                LastName = "Joombar"
            }).Result;

            var emp = service.GetAsync<Employee>(id).Result;

            Assert.IsTrue(emp.Id == id);
        }

        [TestMethod]
        public void SaveEmpWithColumnNames()
        {
            var service = new TestService();

            var id = service.SaveAsync(new Employee()
            {
                FirstName = "Zalarm",
                LastName = "Hoofza",
                IsExempt = true,
                Status = Status.Active
            }, new string[] 
            {
                nameof(Employee.FirstName),
                nameof(Employee.LastName),
                nameof(Employee.IsExempt),
                nameof(Employee.Status)
            }).Result;

            var emp = service.GetAsync<Employee>(id).Result;

            Assert.IsTrue(emp.Id == id);
        }
    }

    public class TestService : SqlCrudService<int, DummyUser>
    {
        public TestService() : base(new SqlServerIntCrudProvider(), "adamo")
        {
        }

        public override IDbConnection GetConnection() => LocalDb.GetConnection("DapperCX");

        protected override DummyUser QueryUser(IDbConnection connection, string userName)
        {
            return new DummyUser(userName);
        }
    }

    public class DummyUser : IUserBase
    {
        public DummyUser(string userName)
        {
            Name = userName;
        }

        public string Name { get; set; }

        public DateTime LocalTime => DateTime.UtcNow;
    }
}
