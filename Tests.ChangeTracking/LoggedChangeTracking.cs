using AO.Models.Interfaces;
using Dapper;
using Dapper.CX.SqlServer;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlServer.LocalDb;
using System;
using System.Collections.Generic;
using System.Linq;
using Tests.ChangeTracking.Models;
using Tests.Models;
using Dapper.CX.Extensions;
using Dapper.CX.Classes;
using ModelSync.Models;

namespace Tests.Base
{
    [TestClass]
    public class LoggedChangeTracking
    {
        private const string dbName = "DapperCX";

        [ClassInitialize]
        public static void Initialize(TestContext context)
        {
            using (var cn = LocalDb.GetConnection(dbName))
            {
                LocalDb.ExecuteInitializeStatements(cn, DbObjects.CreateObjects());
            }
        }

        [TestMethod]
        public void CaptureChanges()
        {
            using (var cn = LocalDb.GetConnection(dbName))
            {
                var emp = new Employee()
                {
                    LastName = "Herbert",
                    FirstName = "Yavad",
                    HireDate = new DateTime(1990, 1, 1),
                    IsExempt = true
                };

                // need to make sure record doesn't exist
                try { cn.Execute("TRUNCATE TABLE [changes].[RowVersion]"); } catch {  /*do nothing */ }
                try { cn.Execute("TRUNCATE TABLE [changes].[ColumnHistory]"); } catch { /* do nothing */ }
                cn.Execute("DELETE [dbo].[Employee] WHERE [FirstName]=@firstName AND [LastName]=@lastName", emp);

                var provider = new SqlServerCrudProvider<int>((id) => Convert.ToInt32(id));
                provider.SaveAsync(cn, emp).Wait();
                LoggedChangeTracker<Employee> ct = GetLoggedChangeTracker(emp);

                emp.FirstName = "Javad";
                emp.Status = Status.Inactive;
                provider.SaveAsync(cn, emp, ct).Wait();

                Assert.IsTrue(cn.RowExistsAsync("[changes].[RowVersion] WHERE [TableName]='Employee' AND [RowId]=1 AND [Version]=1").Result);
                Assert.IsTrue(cn.RowExistsAsync("[changes].[ColumnHistory] WHERE [TableName]='Employee' AND [RowId]=1 AND [Version]=1 AND [ColumnName]='FirstName' AND [OldValue]='Yavad' AND [NewValue]='Javad'").Result);
                Assert.IsTrue(cn.RowExistsAsync("[changes].[ColumnHistory] WHERE [TableName]='Employee' AND [RowId]=1 AND [Version]=1 AND [ColumnName]='Status' AND [OldValue]='Active' AND [NewValue]='Inactive'").Result);
            }
        }

        private static LoggedChangeTracker<TModel> GetLoggedChangeTracker<TModel>(TModel model)
        {
            return new LoggedChangeTracker<TModel>(new DataModel(), new SqlServerCrudProvider<long>((id) => Convert.ToInt64(id)), new LocalUser("adamo"), model);
        }

        [TestMethod]
        public void TextLookupChanges()
        {
            using (var cn = LocalDb.GetConnection(dbName))
            {
                DataModel.CreateTablesAsync(new Type[]
                {
                    typeof(WidgetType),
                    typeof(Widget)
                }, cn).Wait();

                cn.Execute("DELETE [dbo].[Widget]");
                cn.Execute("DELETE [dbo].[WidgetType]");

                var provider = new SqlServerCrudProvider<int>((id) => Convert.ToInt32(id));
                HashSet<int> ids = new HashSet<int>();
                Array.ForEach(new[] { "this", "that", "other" }, (name) =>
                {
                    ids.Add(provider.Save(cn, new WidgetType() { Name = name }));
                });

                var w = new Widget()
                {
                    Description = "this new thing",
                    TypeId = ids.First(),
                    Price = 23.4m
                };

                int widgetId = provider.Save(cn, w);

                var ct = GetLoggedChangeTracker(w);
                w.Price = 21.7m;
                w.TypeId = ids.Last();
                provider.SaveAsync(cn, w, ct).Wait();

                Assert.IsTrue(cn.RowExistsAsync("[changes].[ColumnHistory] WHERE [TableName]='Widget' AND [RowId]=@widgetId AND [ColumnName]='Price' AND [OldValue]='23.4' AND [NewValue]='21.7'", new { widgetId }).Result);
                Assert.IsTrue(cn.RowExistsAsync("[changes].[ColumnHistory] WHERE [TableName]='Widget' AND [RowId]=@widgetId AND [ColumnName]='TypeId' AND [OldValue]='this' AND [NewValue]='other'", new { widgetId }).Result);
            }
        }

        public class LocalUser : IUserBase
        {
            public LocalUser(string userName)
            {
                Name = userName;
            }

            public string Name { get; }
            public DateTime LocalTime => DateTime.UtcNow;
        }
    }
}
