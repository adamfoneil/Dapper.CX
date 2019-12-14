using Dapper.CX.Abstract;
using System;

namespace Dapper.CX.SqlServer.Abstract
{
    public abstract class SqlServerCrudProvider<TIdentity> : SqlCrudProvider<TIdentity>
    {
        protected override char StartDelimiter => '[';
        protected override char EndDelimiter => ']';
        protected override string SelectIdentityCommand => "SELECT SCOPE_IDENTITY();";

        protected override Type[] SupportedTypes => new Type[]
        {
            typeof(string),
            typeof(int), typeof(int?),
            typeof(DateTime), typeof(DateTime?),
            typeof(bool), typeof(bool?),
            typeof(long), typeof(long?),
            typeof(decimal), typeof(decimal?),
            typeof(double), typeof(double?),
            typeof(float), typeof(float?)
        };
    }
}
