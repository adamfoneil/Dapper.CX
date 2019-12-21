using Dapper;
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

        protected void NewObjShouldBeNewBase()
        {
            var emp = GetTestEmployee();
            var provider = GetProvider();
            Assert.IsTrue(provider.IsNew(emp));
        }

        protected void InsertBase()
        {
            using (var cn = GetConnection())
            {
                Employee emp = GetTestEmployee();

                var provider = GetProvider();
                provider.InsertAsync(cn, emp).Wait();
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
        
        protected void DeleteBase()
        {
            using (var cn = GetConnection())
            {
                var emp = GetTestEmployee();
                var provider = GetProvider();
                TIdentity id = provider.InsertAsync(cn, emp).Result;

                provider.DeleteAsync<Employee>(cn, id).Wait();

                emp = provider.GetAsync<Employee>(cn, id).Result;
                Assert.IsNull(emp);
            }
        }

        protected void ExistsBase()
        {
            using (var cn = GetConnection())
            {
                var provider = GetProvider();

                var emp = GetTestEmployee();
                TIdentity id = provider.SaveAsync(cn, emp).Result;

                Assert.IsTrue(provider.ExistsAsync<Employee>(cn, id).Result);
            }
        }

        protected void ExistsWhereBase()
        {
            using (var cn = GetConnection())
            {
                cn.Execute("TRUNCATE TABLE [dbo].[Employee]");

                var provider = GetProvider();

                var emp = GetTestEmployee();
                provider.SaveAsync(cn, emp).Wait();

                var criteria = new { emp.FirstName, emp.LastName };
                Assert.IsTrue(provider.ExistsWhereAsync<Employee>(cn, criteria).Result);
            }
        }

        protected void MergeExplicitPropsBase()
        {
            using (var cn = GetConnection())
            {
                cn.Execute("TRUNCATE TABLE [dbo].[Employee]");

                var emp = GetTestEmployee();
                emp.IsExempt = true;

                var provider = GetProvider();
                TIdentity id = provider.SaveAsync(cn, emp).Result;

                var mergeEmp = GetTestEmployee();
                mergeEmp.IsExempt = false;

                provider.MergeAsync(cn, mergeEmp, new string[] { nameof(Employee.FirstName), nameof(Employee.LastName) }).Wait();

                var findEmp = provider.GetAsync<Employee>(cn, id).Result;
                Assert.IsFalse(findEmp.IsExempt);
            }
        }

        protected void MergePKPropsBase()
        {
            using (var cn = GetConnection())
            {
                cn.Execute("TRUNCATE TABLE [dbo].[Employee]");

                var emp = GetTestEmployee();
                emp.IsExempt = true;

                var provider = GetProvider();
                TIdentity id = provider.SaveAsync(cn, emp).Result;

                var mergeEmp = GetTestEmployee();
                mergeEmp.IsExempt = false;

                provider.MergeAsync(cn, mergeEmp).Wait();

                var findEmp = provider.GetAsync<Employee>(cn, id).Result;
                Assert.IsFalse(findEmp.IsExempt);
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
