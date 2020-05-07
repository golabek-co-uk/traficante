using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using Traficante.TSQL.Evaluator.Tests.Core.Schema;

namespace Traficante.TSQL.Evaluator.Tests.Core
{
    [TestClass]
    public class OrderByTests : TestBase
    {

        [TestMethod]
        public void SimpleOrderByTest()
        {
            var query = @"select Name from #A.Entities() order by Population";
            var sources = new Dictionary<string, IEnumerable<BasicEntity>>
            {
                {
                    "#A",
                    new[]
                    {
                        new BasicEntity { Name = "001", Population = 6 },
                        new BasicEntity { Name = "002", Population = 5 },
                        new BasicEntity { Name = "003", Population = 4 },
                        new BasicEntity { Name = "004", Population = 3 },
                        new BasicEntity { Name = "005", Population = 2 },
                        new BasicEntity { Name = "006", Population = 1 }
                    }
                }
            };

            var vm = CreateAndRunVirtualMachine(query, sources);
            var table = vm.Run();

            Assert.AreEqual(6, table.Count);
            Assert.AreEqual("006", table[0].Values[0]);
            Assert.AreEqual("005", table[1].Values[0]);
        }

        [TestMethod]
        public void SimpleOrderByAscTest()
        {
            var query = @"select Name from #A.Entities() order by Population asc";
            var sources = new Dictionary<string, IEnumerable<BasicEntity>>
            {
                {
                    "#A",
                    new[]
                    {
                        new BasicEntity { Name = "001", Population = 6 },
                        new BasicEntity { Name = "002", Population = 5 },
                        new BasicEntity { Name = "003", Population = 4 },
                        new BasicEntity { Name = "004", Population = 3 },
                        new BasicEntity { Name = "005", Population = 2 },
                        new BasicEntity { Name = "006", Population = 1 }
                    }
                }
            };

            var vm = CreateAndRunVirtualMachine(query, sources);
            var table = vm.Run();

            Assert.AreEqual(6, table.Count);
            Assert.AreEqual("006", table[0].Values[0]);
            Assert.AreEqual("005", table[1].Values[0]);
        }

        [TestMethod]
        public void SimpleOrderByDescTest()
        {
            var query = @"select Name from #A.Entities() order by Population desc";
            var sources = new Dictionary<string, IEnumerable<BasicEntity>>
            {
                {
                    "#A",
                    new[]
                    {
                        new BasicEntity { Name = "001", Population = 6 },
                        new BasicEntity { Name = "002", Population = 5 },
                        new BasicEntity { Name = "003", Population = 4 },
                        new BasicEntity { Name = "004", Population = 3 },
                        new BasicEntity { Name = "005", Population = 2 },
                        new BasicEntity { Name = "006", Population = 1 }
                    }
                }
            };

            var vm = CreateAndRunVirtualMachine(query, sources);
            var table = vm.Run();

            Assert.AreEqual(6, table.Count);
            Assert.AreEqual("001", table[0].Values[0]);
            Assert.AreEqual("002", table[1].Values[0]);
        }
        [TestMethod]
        public void SimpleOrderByDesAscTest()
        {
            var query = @"select Name from #A.Entities() order by Population desc, Money asc";
            var sources = new Dictionary<string, IEnumerable<BasicEntity>>
            {
                {
                    "#A",
                    new[]
                    {
                        new BasicEntity { Name = "001", Population = 6, Money = 1 },
                        new BasicEntity { Name = "002", Population = 5, Money = 2 },
                        new BasicEntity { Name = "004", Population = 4, Money = 3 },
                        new BasicEntity { Name = "003", Population = 3, Money = 5 },
                        new BasicEntity { Name = "005", Population = 2, Money = 4 },
                        new BasicEntity { Name = "006", Population = 6, Money = 6 }
                    }
                }
            };

            var vm = CreateAndRunVirtualMachine(query, sources);
            var table = vm.Run();

            Assert.AreEqual(6, table.Count);
            Assert.AreEqual("001", table[0].Values[0]);
            Assert.AreEqual("006", table[1].Values[0]);
            Assert.AreEqual("002", table[2].Values[0]);
        }

        [TestMethod]
        public void SimpleOrderByDescDescTest()
        {
            var query = @"select Name from #A.Entities() order by Population desc, Money desc";
            var sources = new Dictionary<string, IEnumerable<BasicEntity>>
            {
                {
                    "#A",
                    new[]
                    {
                        new BasicEntity { Name = "001", Population = 6, Money = 1 },
                        new BasicEntity { Name = "002", Population = 5, Money = 2 },
                        new BasicEntity { Name = "004", Population = 4, Money = 3 },
                        new BasicEntity { Name = "003", Population = 3, Money = 5 },
                        new BasicEntity { Name = "005", Population = 2, Money = 4 },
                        new BasicEntity { Name = "006", Population = 6, Money = 6 }
                    }
                }
            };

            var vm = CreateAndRunVirtualMachine(query, sources);
            var table = vm.Run();

            Assert.AreEqual(6, table.Count);
            Assert.AreEqual("006", table[0].Values[0]);
            Assert.AreEqual("001", table[1].Values[0]);
            Assert.AreEqual("002", table[2].Values[0]);
        }

        [TestMethod]
        public void SimpleTopWithOrderByTest()
        {
            var query = @"select top 2 Name from #A.Entities() order by Population asc";
            var sources = new Dictionary<string, IEnumerable<BasicEntity>>
            {
                {
                    "#A",
                    new[]
                    {
                        new BasicEntity { Name = "001", Population = 6 },
                        new BasicEntity { Name = "002", Population = 5 },
                        new BasicEntity { Name = "003", Population = 4 },
                        new BasicEntity { Name = "004", Population = 3 },
                        new BasicEntity { Name = "005", Population = 2 },
                        new BasicEntity { Name = "006", Population = 1 }
                    }
                }
            };

            var vm = CreateAndRunVirtualMachine(query, sources);
            var table = vm.Run();

            Assert.AreEqual(2, table.Count);
            Assert.AreEqual("006", table[0].Values[0]);
            Assert.AreEqual("005", table[1].Values[0]);
        }
    }

}
