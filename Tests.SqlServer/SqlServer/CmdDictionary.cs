using Dapper.CX.Classes;
using Dapper.CX.SqlServer;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tests.Extensions;

namespace Tests.SqlServer
{
    [TestClass]
    public class CmdDictionary
    {
        [TestMethod]
        public void SqlServerInsertCommand()
        {
            var ins = new SqlServerCmd("dbo.Table1", "Id")
            {
                { "FirstName", "Adam" },
                { "LastName", "O'Neil" }
            };

            var cmd = ins.GetInsertStatement();
            Assert.IsTrue(cmd.ReplaceWhitespace().Equals(
                @"INSERT INTO [dbo].[Table1] (
                    [FirstName], [LastName]
                ) VALUES (
                    @FirstName, @LastName
                ); SELECT SCOPE_IDENTITY();".ReplaceWhitespace()));
        }

        [TestMethod]
        public void SqlServerUpdateCommand()
        {
            var upd = new SqlServerCmd("dbo.Table1", "Id")
            {
                { "FirstName", "Adam" },
                { "LastName", "O'Neil" }
            };

            var cmd = upd.GetUpdateStatement();
            Assert.IsTrue(cmd.ReplaceWhitespace().Equals(
                @"UPDATE [dbo].[Table1] SET
                    [FirstName]=@FirstName, [LastName]=@LastName
                WHERE [Id]=@Id".ReplaceWhitespace()));
        }

        [TestMethod]
        public void SqlServerUpdateCommandWithKeyColumns()
        {
            var upd = new SqlServerCmd("dbo.Table1", "Id")
            {
                { "FirstName", "Adam" },
                { "LastName", "O'Neil" }
            };

            var cmd = upd.GetUpdateStatement();
            Assert.IsTrue(cmd.ReplaceWhitespace().Equals(
                @"UPDATE [dbo].[Table1] SET
                    [FirstName]=@FirstName, [LastName]=@LastName
                WHERE [Id]=@Id".ReplaceWhitespace()));
        }

        [TestMethod]
        public void SqlServerUpdateWithExpression()
        {
            var upd = new SqlServerCmd("dbo.Table1", "Id")
            {
                { "FirstName", "Adam" },
                { "LastName", "O'Neil" },
                { "Weight", new SqlExpression("[Weight]-10") } // lost 10 lbs, yay!!!
            };

            var cmd = upd.GetUpdateStatement();
            Assert.IsTrue(cmd.ReplaceWhitespace().Equals(
                @"UPDATE [dbo].[Table1] SET
                    [FirstName]=@FirstName, [LastName]=@LastName, [Weight]=[Weight]-10
                WHERE [Id]=@Id".ReplaceWhitespace()));
        }

        [TestMethod]
        public void SqlServerInsertWithExpression()
        {
            var ins = new SqlServerCmd("dbo.Table1", "Id")
            {
                { "FirstName", "Adam" },
                { "LastName", "O'Neil" },
                { "CurrentDate", new SqlExpression("getdate()") }
            };

            var cmd = ins.GetInsertStatement();
            Assert.IsTrue(cmd.ReplaceWhitespace().Equals(
                @"INSERT INTO [dbo].[Table1] (
                    [FirstName], [LastName], [CurrentDate]
                ) VALUES (
                    @FirstName, @LastName, getdate()
                ); SELECT SCOPE_IDENTITY();".ReplaceWhitespace()));
        }

        [TestMethod]
        public void SqlServerCmdKeyColumns()
        {
            var cmd = new SqlServerCmd("dbo.Table1", "Id")
            {
                ["#KeyValue"] = "hello",
                ["AnotherValue"] = "whatever"
            };

            Assert.IsTrue(cmd["#KeyValue"].Equals(cmd["KeyValue"]));
            Assert.IsTrue(cmd["KeyValue"].Equals("hello"));
            Assert.IsTrue(cmd["#KeyValue"].Equals("hello"));
        }
    }
}
