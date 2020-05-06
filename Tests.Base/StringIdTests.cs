using Dapper.CX.Static;
using JsonSettings;
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
                string id = StringId.New(9);
                if (!ids.Add(id)) dups.Add(id);
            }            

            Assert.IsTrue(ids.Count == count);

            // no dups in a million -- good enough for now!
        }
    }
}
