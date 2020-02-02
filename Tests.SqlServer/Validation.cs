using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Tests.SqlServer.Models;

namespace Tests.SqlServer
{
    [TestClass]
    public class Validation
    {
        [TestMethod]
        public void InvalidEmployee()
        {
            var emp = new EmployeeValid()
            {
                FirstName = "nobody",
                LastName = "whatever",
                HireDate = DateTime.Today,
                TermDate = DateTime.Today.AddDays(-30)
            };

            var result = emp.Validate();
            Assert.IsTrue(result.IsValid == false);
        }
    }
}
