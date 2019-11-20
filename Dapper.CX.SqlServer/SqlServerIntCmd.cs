using Dapper.CX.SqlServer.Abstract;
using System;

namespace Dapper.CX.SqlServer
{
    public class SqlServerIntCmd : SqlServerCmd<int>
    {
        public SqlServerIntCmd(string tableName, string identityColumn) : base(tableName, identityColumn)
        {
        }

        public SqlServerIntCmd(object @object) : base(@object)
        {
        }

        public SqlServerIntCmd(Type type) : base(type)
        {
        }        

        protected override int ConvertIdentity(object identity)
        {
            return Convert.ToInt32(identity);
        }
    }
}
