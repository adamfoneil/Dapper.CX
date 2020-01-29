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
                    [Id] int identity(1, 1) PRIMARY KEY
                )");
        }

    }
}
