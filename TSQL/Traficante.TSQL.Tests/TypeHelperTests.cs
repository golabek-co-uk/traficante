using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

namespace Traficante.TSQL.Tests
{
    [TestClass]
    public class TypeHelperTests
    {
        [TestMethod]
        public void Test()
        {
            //var props = TypeHelper.GetAccessors(typeof(TypeHelperClass));
            //int id = (int)(props["Id"]((object)new TypeHelperClass { Id = 12 }));
            //Assert.AreEqual(12, id);
        }
    }

    public class TypeHelperClass
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
