using Dapper.CX.Abstract;
using Dapper.CX.SqlServer;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlServer.LocalDb;
using System.Data;
using Tests.Models;

namespace Tests.Base
{
    [TestClass]
    public class ServiceTests
    {
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

    public class TestService : SqlCrudService<int>
    {
        public TestService() : base(new SqlServerIntCrudProvider())
        {
        }

        public override IDbConnection GetConnection() => LocalDb.GetConnection("DapperCX");
    }
}
