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

            yield return new InitializeStatement(
                "dbo.AspNetRoles", "DROP TABLE %obj%",
                @"CREATE TABLE [dbo].[AspNetRoles](
	                [Id] [nvarchar](450) NOT NULL,
	                [Name] [nvarchar](256) NULL,
	                [NormalizedName] [nvarchar](256) NULL,
	                [ConcurrencyStamp] [nvarchar](max) NULL,
                 CONSTRAINT [PK_AspNetRoles] PRIMARY KEY CLUSTERED 
                (
	                [Id] ASC
                ))");

            yield return new InitializeStatement(
                "dbo.AspNetUserRoles", "DROP TABLE %obj%",
                @"CREATE TABLE [dbo].[AspNetUserRoles](
	                [UserId] [nvarchar](450) NOT NULL,
	                [RoleId] [nvarchar](450) NOT NULL,
                 CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY CLUSTERED 
                (
	                [UserId] ASC,
	                [RoleId] ASC
                ))");

            yield return new InitializeStatement(
                "dbo.AspNetUsers", "DROP TABLE %obj%",
                @"CREATE TABLE [dbo].[AspNetUsers](
	                [Id] [nvarchar](450) NOT NULL,
	                [UserName] [nvarchar](256) NULL,
	                [NormalizedUserName] [nvarchar](256) NULL,
	                [Email] [nvarchar](256) NULL,
	                [NormalizedEmail] [nvarchar](256) NULL,
	                [EmailConfirmed] [bit] NOT NULL,
	                [PasswordHash] [nvarchar](max) NULL,
	                [SecurityStamp] [nvarchar](max) NULL,
	                [ConcurrencyStamp] [nvarchar](max) NULL,
	                [PhoneNumber] [nvarchar](max) NULL,
	                [PhoneNumberConfirmed] [bit] NOT NULL,
	                [TwoFactorEnabled] [bit] NOT NULL,
	                [LockoutEnd] [datetimeoffset](7) NULL,
	                [LockoutEnabled] [bit] NOT NULL,
	                [AccessFailedCount] [int] NOT NULL,
                    CONSTRAINT [PK_AspNetUsers] PRIMARY KEY CLUSTERED 
                    (
	                    [Id] ASC
                    ))");
        }
    }
}
