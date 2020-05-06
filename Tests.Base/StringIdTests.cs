using Dapper.CX.Static;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;

namespace Tests.Base
{
    [TestClass]
    public class StringIdTests
    {
        [TestMethod]
        public void GenerateStringIds()
        {
            HashSet<string> ids = new HashSet<string>();
            List<string> dups = new List<string>();

            const int count = 1_000_000;
            
            for (int i = 0; i < count; i++)
            {
                string id = StringId.New(12);
                if (!ids.Add(id)) dups.Add(id);
            }

            var dupGrp = dups.GroupBy(s => s);

            Assert.IsTrue(ids.Count == count);

            // dup rate is about ‭0.00025‬
        }
    }
}
