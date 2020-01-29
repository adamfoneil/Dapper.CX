using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlServer.LocalDb;

namespace Tests.Base
{
    [TestClass]
    public class GetRelated
    {
        [TestMethod]
        public void GetEmployeeWithRelated()
        {
            using (var cn = LocalDb.GetConnection("DapperCX"))
            {

            }
        }
    }
}
