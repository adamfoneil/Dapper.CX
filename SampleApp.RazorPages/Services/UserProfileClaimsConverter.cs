﻿using Dapper;
using Dapper.CX.SqlServer.Services;
using Microsoft.Data.SqlClient;
using SampleApp.Models;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SampleApp.RazorPages.Services
{
    public class UserProfileClaimsConverter : DbUserClaimsConverter<UserProfile>
    {
        private readonly string _connectionString;

        public UserProfileClaimsConverter(string connectionString)
        {
            _connectionString = connectionString;
        }

        public override IEnumerable<Claim> GetClaimsFromUser(UserProfile user)
        {
            yield return new Claim(nameof(UserProfile.WorkspaceId), (user.WorkspaceId ?? 0).ToString());
            yield return new Claim(nameof(UserProfile.WorkspaceName), user.WorkspaceName);
            yield return new Claim(nameof(UserProfile.TimeZoneId), user.TimeZoneId);
            yield return new Claim(nameof(UserProfile.DisplayName), user.DisplayName);
            yield return new Claim(nameof(UserProfile.UserId), user.UserId.ToString());
            yield return new Claim(nameof(UserProfile.Email), user.Email);
        }

        public override UserProfile GetUserFromClaims(string userName, IEnumerable<Claim> claims)
        {
            var result = Parse(claims);
            result.UserName = userName;
            return result;            
        }

        public override async Task<UserProfile> QueryUserAsync(string userName)
        {
            using (var cn = new SqlConnection(_connectionString))
            {
                return await cn.QuerySingleAsync<UserProfile>(
                    @"SELECT [u].*, [ws].[Name] AS [WorkspaceName]
                    FROM [dbo].[AspNetUsers] [u] 
                    LEFT JOIN [dbo].[Workspace] [ws] ON [u].[WorkspaceId]=[ws].[Id]
                    WHERE [UserName]=@userName",
                    new { userName });
            }
        }
    }
}
