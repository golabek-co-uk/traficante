using Microsoft.VisualStudio.TestTools.UnitTesting;
using Traficante.TSQL.Schema.DataSources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Traficante.TSQL.Evaluator.Tests.Core
{
    [TestClass]
    public class SourceTests : TestBase
    {
        //[TestMethod]
        //public void SelectFromDictionaryTest()
        //{
        //    var query = @"select * from #A.Entities()";


        //    var sources = new Dictionary<string, IEnumerable<IDictionary<string, object>>>
        //    {
        //        {"#A", new[]
        //            {
        //                new Dictionary<string, object> { { "Id", 1 }, { "Name", "Jon" } },
        //                new Dictionary<string, object> { { "Id", 2 }, { "Name", "Mark" } },
        //                new Dictionary<string, object> { { "Id", 3 }, { "Name", "Adam" } }
        //            }
        //        },
        //    };

        //    var vm = CreateAndRunVirtualMachine(query, sources);
        //    var table = vm.Run();

        //    Assert.AreEqual(3, table.Count);
        //    //Assert.AreEqual("002", table[0].Values[0]);
        //    //Assert.AreEqual("001", table[1].Values[0]);
        //}
    }
}
