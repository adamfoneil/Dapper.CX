using Dapper.CX.Abstract;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Data;
using Tests.Models;

namespace Tests
{
    public abstract class IntegrationBase<TIdentity>
    {
        protected abstract IDbConnection GetConnection();

        protected abstract SqlCrudProvider<TIdentity> GetProvider();

        protected void InsertBase()
        {
            using (var cn = GetConnection())
            {
                Employee emp = GetTestEmployee();

                var provider = GetProvider();
                TIdentity id = provider.InsertAsync(cn, emp).Result;
                Assert.IsTrue(emp.Id != default);
            }
        }

        protected void UpdateBase()
        {
            using (var cn = GetConnection())
            {
                var emp = GetTestEmployee();
                
                var provider = GetProvider();                
                TIdentity id = provider.InsertAsync(cn, emp).Result;

                const string newName = "Wonga";
                emp.FirstName = newName;
                provider.UpdateAsync(cn, emp).Wait();

                emp = provider.GetAsync<Employee>(cn, id).Result;
                Assert.IsTrue(emp.FirstName.Equals(newName));
            }
        }

        private static Employee GetTestEmployee()
        {
            return new Employee()
            {
                FirstName = "Test",
                LastName = "Person",
                HireDate = DateTime.Today,
                IsExempt = true
            };
        }
    }
}
