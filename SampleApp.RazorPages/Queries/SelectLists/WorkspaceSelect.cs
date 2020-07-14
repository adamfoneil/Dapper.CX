using Dapper.QX;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace SampleApp.RazorPages.Queries.SelectLists
{
    public class WorkspaceSelect : Query<SelectListItem>
    {
        public WorkspaceSelect() : base("SELECT [Id] AS [Value], [Name] AS [Text] FROM [dbo].[Workspace] ORDER BY [Name]")
        {
        }
    }
}
