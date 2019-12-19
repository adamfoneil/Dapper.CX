using Dapper.CX.Abstract;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Tests.SqlServer
{
    [TestClass]
    public class SqlServerIntegrationInt : IntegrationBase<int>
    {
        protected override IDbConnection GetConnection()
        {
            throw new NotImplementedException();
        }

        protected override SqlCrudProvider<int> GetProvider()
        {
            throw new NotImplementedException();
        }
    }
}
