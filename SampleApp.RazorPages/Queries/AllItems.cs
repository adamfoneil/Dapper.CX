using Dapper.QX;
using SampleApp.Models;

namespace SampleApp.RazorPages.Queries
{
    public class AllItems : Query<Item>
    {
        public AllItems() : base("SELECT * FROM [dbo].[Item] WHERE [IsActive]=@isActive AND [WorkspaceId]=@workspaceId ORDER BY [Name]")
        {
        }

        public int WorkspaceId { get; set; }
        public bool IsActive { get; set; }
    }
}
