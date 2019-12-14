using Dapper.CX.SqlServer.Abstract;
using System;

namespace Dapper.CX.SqlServer
{
    public class SqlServerLongCrudProvider : SqlServerCrudProvider<long>
    {
        protected override long ConvertIdentity(object identity)
        {
            return Convert.ToInt64(identity);
        }
    }
}
