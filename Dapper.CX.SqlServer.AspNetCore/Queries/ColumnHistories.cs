using Dapper.CX.Models;
using Dapper.QX;
using Dapper.QX.Attributes;

namespace Dapper.CX.SqlServer.AspNetCore.Queries
{
    public class ColumnHistories<TModel> : Query<ColumnHistory>
    {
        public ColumnHistories() : base("SELECT * FROM [changes].[ColumnHistory] WHERE [TableName]=@tableName {andWhere} ORDER BY [Version]")
        {
            TableName = typeof(TModel).Name;
        }

        [Parameter]
        public string TableName { get; }

        [Where("[RowId]=@rowId")]
        public long? RowId { get; set; }
    }
}
