using Dapper.CX.Static;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

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

            Assert.IsTrue(ids.Count == count);

            // dup rate is about ‭0.00025‬ or one every 4000, which I don't consider good enough
        }
    }
}
