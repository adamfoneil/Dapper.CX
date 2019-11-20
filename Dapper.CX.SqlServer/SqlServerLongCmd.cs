using Dapper.CX.SqlServer.Abstract;
using System;

namespace Dapper.CX.SqlServer
{
    public class SqlServerLongCmd : SqlServerCmd<long>
    {
        public SqlServerLongCmd(string tableName, string identityColumn) : base(tableName, identityColumn)
        {
        }

        public SqlServerLongCmd(object @object) : base(@object)
        {
        }

        public SqlServerLongCmd(Type type) : base(type)
        {
        }

        protected override long ConvertIdentity(object identity)
        {
            return Convert.ToInt64(identity);
        }
    }
}
