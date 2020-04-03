using SqlServer.LocalDb.Models;
using System.Collections.Generic;

namespace Tests.Models
{
    public static class DbObjects
    {
        public static IEnumerable<InitializeStatement> CreateObjects()
        {
            yield return new InitializeStatement(
                "dbo.Employee",
                "DROP TABLE %obj%",
                @"CREATE TABLE %obj% (
                    [FirstName] nvarchar(50) NOT NULL,
                    [LastName] nvarchar(50) NOT NULL,
                    [HireDate] date NULL,
                    [TermDate] date NULL,
                    [IsExempt] bit NOT NULL,
                    [Timestamp] datetime NULL,
                    [Status] int NOT NULL,
                    [Value] int NULL,
                    [Id] int identity(1, 1) PRIMARY KEY
                )");

            yield return new InitializeStatement(
                "dbo.SomethingElse", "DROP TABLE %obj%",
                @"CREATE TABLE %obj% (
                    [EmployeeId] int NOT NULL,
                    [Balance] decimal NULL,
                    [Whatever] nvarchar(50) NULL,
                    [Id] int identity(1,1) PRIMARY KEY
                )");
        }
    }
}
