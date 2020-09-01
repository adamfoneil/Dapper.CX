using Dapper.QX;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace SampleApp.RazorPages.Queries.SelectLists
{
    public class ItemSelect : Query<SelectListItem>
    {
        public ItemSelect() : base(
            @"SELECT [Id] AS [Value], [Name] AS [Text]
            FROM [dbo].[Item]
            WHERE [WorkspaceId]=@workspaceId
            ORDER BY [Name]")
        {
        }

        public int WorkspaceId { get; set; }
    }
}
