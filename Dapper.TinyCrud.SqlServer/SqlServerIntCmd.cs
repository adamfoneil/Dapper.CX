using Dapper.TinyCrud.SqlServer.Abstract;
using System;

namespace Dapper.TinyCrud.SqlServer
{
    public class SqlServerIntCmd : SqlServerCmd<int>
    {
        public SqlServerIntCmd()
        {
        }

        public SqlServerIntCmd(object @object)
        {
            Initialize(@object);
        }

        protected override int ConvertIdentity(object identity)
        {
            return Convert.ToInt32(identity);
        }
    }
}
