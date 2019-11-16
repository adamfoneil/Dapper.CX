using Dapper.TinyCrud.Abstract;
using System;
using System.Data;

namespace Dapper.TinyCrud.SqlServer
{
    public class SqlServerCmd : SqlCmdDictionary
    {
        protected override string SelectIdentityCommand => "SELECT SCOPE_IDENTITY()";

        protected override char StartDelimiter => '[';

        protected override char EndDelimiter => ']';

        protected override Type[] SupportedTypes => new Type[] 
        {
            typeof(string), 
            typeof(int),
            typeof(DateTime),
            typeof(bool),
            typeof(long),
            typeof(decimal),
            typeof(double),
            typeof(float)
        };
            

        public override IDbCommand GetInsertCommand(IDbConnection connection)
        {
            throw new NotImplementedException();
        }

        public override IDbCommand GetUpdateCommand(IDbConnection connection)
        {
            throw new NotImplementedException();
        }
    }
}
