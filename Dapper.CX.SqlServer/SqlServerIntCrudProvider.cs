using Dapper.CX.SqlServer.Abstract;
using System;

namespace Dapper.CX.SqlServer
{
    public class SqlServerIntCrudProvider : SqlServerCrudProvider<int>
    {
        protected override int ConvertIdentity(object identity) => Convert.ToInt32(identity);
    }
}
