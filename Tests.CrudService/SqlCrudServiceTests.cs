using Dapper.CX.Models;
using Dapper.CX.SqlServer;
using Dapper.CX.SqlServer.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModelSync.Models;
using SampleApp.Models;
using SqlServer.LocalDb;
using System;

namespace Tests.CrudService
{
    [TestClass]
    public class SqlCrudServiceTests
    {
        const string dbName = "SampleApp.Tests";
        const string userName = "test.process";
        const string wsName = "Sample WS";

        [ClassInitialize]
        public static void InitializeDb(TestContext context)
        {
            LocalDb.TryDropDatabaseIfExists(dbName, out _);

            DataModel.CreateTablesAsync(new Type[]
            {
                typeof(Item),
                typeof(Workspace),
                typeof(WorkspaceUser),
                typeof(UserProfile),
                typeof(ColumnHistory),
                typeof(RowVersion)
            }, LocalDb.GetConnection(dbName)).Wait();            

            using (var cn = LocalDb.GetConnection(dbName))
            {
                GetProvider().Insert(cn, new Workspace()
                {
                    Name = wsName,
                    CreatedBy = userName,
                    DateCreated = DateTime.UtcNow
                });

                GetProvider().Insert(cn, GetUser());
            }
        }

        private static UserProfile GetUser() => new UserProfile()
        {
            UserName = userName,
            Email = "adamosoftware@gmail.com",
            TimeZoneId = "Eastern Standard Time",
            DisplayName = "adamo",
            WorkspaceId = 1
        };

        private static SqlServerCrudProvider<int> GetProvider() => new SqlServerCrudProvider<int>((id) => Convert.ToInt32(id));

        private static DapperCX<int, UserProfile> GetService() => new DapperCX<int, UserProfile>(LocalDb.GetConnectionString(dbName), GetUser(), GetProvider());

        [TestMethod]
        public void GetAsync()
        {
            var result = GetService().GetAsync<Workspace>(1).Result;
            Assert.IsTrue(result.Name.Equals(wsName));
        }

        [TestMethod]
        public void GetWhereAsync()
        {
            var result = GetService().GetWhereAsync<Workspace>(new { name = wsName }).Result;
            Assert.IsTrue(result.Name.Equals(wsName));
        }

        [TestMethod]
        public void ExistsAsync()
        {
            var exists = GetService().ExistsAsync<Workspace>(1).Result;
            Assert.IsTrue(exists);
        }

        [TestMethod]
        public void ExistsWhereAsync()
        {
            var exists = GetService().ExistsWhereAsync<Workspace>(new { name = wsName }).Result;
            Assert.IsTrue(exists);
        }

        [TestMethod]
        public void SaveAsync()
        {
            var id = GetService().SaveAsync(new Item()
            {
                WorkspaceId = 1,
                Name = "item one",
                UnitCost = 15m,
                SalePrice = 25m
            }).Result;

            Assert.IsFalse(id.Equals(default));

            var savedItem = GetService().GetAsync<Item>(id).Result;
            Assert.IsTrue(savedItem.CreatedBy.Equals(userName));
        }

        [TestMethod]
        public void SaveSelectColumnsAsync()
        {
            var service = GetService();

            var id = service.SaveAsync(new Item()
            {
                WorkspaceId = 1,
                Name = "item two",
                UnitCost = 14m,
                SalePrice = 23m
            }).Result;
            
            var savedItem = service.GetAsync<Item>(id).Result;
            savedItem.Name = "whatever";
            savedItem.UnitCost = 12;
            savedItem.SalePrice = 22;

            service.SaveAsync(savedItem, new string[] { nameof(Item.UnitCost), nameof(Item.SalePrice) }).Wait();

            savedItem = service.GetAsync<Item>(id).Result;
            Assert.IsTrue(savedItem.Name.Equals("item two")); // this column should not be affected
            Assert.IsTrue(savedItem.UnitCost == 12);
            Assert.IsTrue(savedItem.SalePrice == 22);
            Assert.IsTrue(savedItem.ModifiedBy == null); // this column should not be affected
        }

        [TestMethod]
        public void SaveStampUpdate()
        {
            var service = GetService();

            var id = service.SaveAsync(new Item()
            {
                WorkspaceId = 1,
                Name = "item three",
                UnitCost = 17m,
                SalePrice = 27m
            }).Result;

            var savedItem = service.GetAsync<Item>(id).Result;            
            savedItem.UnitCost = 18;
            savedItem.SalePrice = 28;

            service.SaveAsync(savedItem).Wait();
            savedItem = service.GetAsync<Item>(id).Result;
            Assert.IsTrue(savedItem.ModifiedBy.Equals(userName));
        }

        [TestMethod]
        public void MergeAsync()
        {
            var service = GetService();

            var id = service.MergeAsync(new Item()
            {
                WorkspaceId = 1,
                Name = "item four",
                UnitCost = 16.5m,
                SalePrice = 19.35m
            }).Result;

            Assert.IsFalse(id.Equals(default));
        }

        [TestMethod]
        public void DeleteAsync()
        {
            var service = GetService();

            var id = service.MergeAsync(new Item()
            {
                WorkspaceId = 1,
                Name = "item five",
                UnitCost = 16.5m,
                SalePrice = 19.35m
            }).Result;

            service.DeleteAsync<Item>(id).Wait();

            Assert.IsTrue(!service.ExistsAsync<Item>(id).Result);
        }
    }
}
