using Dapper.QX.Abstract;
using Dapper.QX.Attributes;
using Dapper.QX.Interfaces;
using System;
using System.Collections.Generic;

namespace SampleApp.Models.Queries
{
    public class WorkspaceUsersResult
    {
        public int Id { get; set; }
        public int WorkspaceId { get; set; }
        public int UserId { get; set; }
        public string CreatedBy { get; set; }
        public DateTime DateCreated { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? DateModified { get; set; }
        public string WorkspaceName { get; set; }
    }

    public class WorkspaceUsers : TestableQuery<WorkspaceUsersResult>
    {
        public WorkspaceUsers() : base(
            @"SELECT 
                [wu].*, [ws].[Name]
            FROM 
                [dbo].[WorkspaceUser] [wu]
                INNER JOIN [dbo].[Workspace] [ws] ON [wu].[WorkspaceId]=[ws].[Id] 
            {where}")
        {
        }

        [Where("[WorkspaceId]=@workspaceId")]
        public int? WorkspaceId { get; set; }

        [Where("[UserId]=@userId")]
        public int? UserId { get; set; }

        [Where("[Status]=@status")]
        public UserStatus? Status { get; set; }

        protected override IEnumerable<ITestableQuery> GetTestCasesInner()
        {
            yield return new WorkspaceUsers() { WorkspaceId = -1 };
            yield return new WorkspaceUsers() { UserId = -1 };
            yield return new WorkspaceUsers() { Status = UserStatus.Enabled };
        }
    }
}
