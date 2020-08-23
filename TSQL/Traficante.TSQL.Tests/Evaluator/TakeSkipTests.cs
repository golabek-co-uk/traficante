using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using Traficante.TSQL.Evaluator.Tests.Core.Schema;
using System.Linq;

namespace Traficante.TSQL.Evaluator.Tests.Core
{
    [TestClass]
    public class DescTests : TestBase
    {
        [TestMethod]
        [Ignore]
        public void DescEntityTest()
        {
            var query = "desc #A.entities()";

            var sources = new Dictionary<string, IEnumerable<BasicEntity>>
            {
                {
                    "#A", new[]
                    {
                        new BasicEntity("xX")
                    }
                }
            };

            var vm = CreateAndRunVirtualMachine(query, sources);
            var table = vm.Run();

            Assert.AreEqual(3, table.Columns.Count());

            Assert.AreEqual("Name", table.Columns.ElementAt(0).ColumnName);
            Assert.AreEqual(typeof(string), table.Columns.ElementAt(0).ColumnType);

            Assert.AreEqual("Index", table.Columns.ElementAt(1).ColumnName);
            Assert.AreEqual(typeof(int), table.Columns.ElementAt(1).ColumnType);

            Assert.AreEqual("Type", table.Columns.ElementAt(2).ColumnName);
            Assert.AreEqual(typeof(string), table.Columns.ElementAt(2).ColumnType);

            Assert.IsTrue(table.Any(row => (string)row[0] == "Name" && (string)row[2] == "System.String"));
            Assert.IsTrue(table.Any(row => (string)row[0] == "City" && (string)row[2] == "System.String"));
            Assert.IsTrue(table.Any(row => (string)row[0] == "Country" && (string)row[2] == "System.String"));
            Assert.IsTrue(table.Any(row => (string)row[0] == "Self" && (string)row[2] == "Traficante.TSQL.Evaluator.Tests.Core.Schema.BasicEntity"));
            Assert.IsTrue(table.Any(row => (string)row[0] == "Money" && (string)row[2] == "System.Decimal"));
            Assert.IsTrue(table.Any(row => (string)row[0] == "Month" && (string)row[2] == "System.String"));
            Assert.IsTrue(table.Any(row => (string)row[0] == "Time" && (string)row[2] == "System.DateTime"));
            Assert.IsTrue(table.Any(row => (string)row[0] == "Id" && (string)row[2] == "System.Int32"));
            Assert.IsTrue(table.Any(row => (string)row[0] == "NullableValue" && (string)row[2] == "System.Nullable`1[System.Int32]"));
        }

        [TestMethod]
        [Ignore]
        public void DescMethodTest()
        {
            var query = "desc #A.entities";

            var sources = new Dictionary<string, IEnumerable<BasicEntity>>
            {
                {
                    "#A", new[]
                    {
                        new BasicEntity("xX")
                    }
                }
            };

            var vm = CreateAndRunVirtualMachine(query, sources);
            var table = vm.Run();

            Assert.AreEqual(1, table.Columns.Count());
            Assert.AreEqual(1, table.Count);
            Assert.AreEqual("entities", table[0][0]);
        }

        [TestMethod]
        [Ignore]
        public void DescSchemaTest()
        {
            var query = "desc #A";

            var sources = new Dictionary<string, IEnumerable<BasicEntity>>
            {
                {
                    "#A", new[]
                    {
                        new BasicEntity("xX")
                    }
                }
            };

            var vm = CreateAndRunVirtualMachine(query, sources);
            var table = vm.Run();

            Assert.AreEqual(1, table.Columns.Count());
            Assert.AreEqual(2, table.Count);

            Assert.AreEqual("empty", table[0][0]);
            Assert.AreEqual("entities", table[1][0]);
        }
    }

    [TestClass]
    public class TakeSkipTests : TestBase
    {
        [TestMethod]
        public void SimpleSkipTest()
        {
            var query = @"select Name from #A.Entities() skip 2";
            var sources = new Dictionary<string, IEnumerable<BasicEntity>>
            {
                {
                    "#A",
                    new[]
                    {
                        new BasicEntity("001"), new BasicEntity("002"), new BasicEntity("003"), new BasicEntity("004"),
                        new BasicEntity("005"), new BasicEntity("006")
                    }
                }
            };

            var vm = CreateAndRunVirtualMachine(query, sources);
            var table = vm.Run();

            Assert.AreEqual(4, table.Count);
            Assert.AreEqual("003", table[0].Values[0]);
            Assert.AreEqual("004", table[1].Values[0]);
            Assert.AreEqual("005", table[2].Values[0]);
            Assert.AreEqual("006", table[3].Values[0]);
        }

        [TestMethod]
        public void SimpleTakeTest()
        {
            var query = @"select Name from #A.Entities() take 2";
            var sources = new Dictionary<string, IEnumerable<BasicEntity>>
            {
                {
                    "#A",
                    new[]
                    {
                        new BasicEntity("001"),
                        new BasicEntity("002"),
                        new BasicEntity("003"),
                        new BasicEntity("004"),
                        new BasicEntity("005"),
                        new BasicEntity("006")
                    }
                }
            };

            var vm = CreateAndRunVirtualMachine(query, sources);
            var table = vm.Run();

            Assert.AreEqual(2, table.Count);
            Assert.AreEqual("001", table[0].Values[0]);
            Assert.AreEqual("002", table[1].Values[0]);
        }

        [TestMethod]
        public void SimpleTopTest()
        {
            var query = @"select top 2 Name from #A.Entities()";
            var sources = new Dictionary<string, IEnumerable<BasicEntity>>
            {
                {
                    "#A",
                    new[]
                    {
                        new BasicEntity("001"),
                        new BasicEntity("002"),
                        new BasicEntity("003"),
                        new BasicEntity("004"),
                        new BasicEntity("005"),
                        new BasicEntity("006")
                    }
                }
            };

            var vm = CreateAndRunVirtualMachine(query, sources);
            var table = vm.Run();

            Assert.AreEqual(2, table.Count);
            Assert.AreEqual("001", table[0].Values[0]);
            Assert.AreEqual("002", table[1].Values[0]);
        }



        [TestMethod]
        public void SimpleSkipTakeTest()
        {
            var query = @"select Name from #A.Entities() skip 1 take 2";
            var sources = new Dictionary<string, IEnumerable<BasicEntity>>
            {
                {
                    "#A",
                    new[]
                    {
                        new BasicEntity("001"), new BasicEntity("002"), new BasicEntity("003"), new BasicEntity("004"),
                        new BasicEntity("005"), new BasicEntity("006")
                    }
                }
            };

            var vm = CreateAndRunVirtualMachine(query, sources);
            var table = vm.Run();

            Assert.AreEqual(2, table.Count);
            Assert.AreEqual("002", table[0].Values[0]);
            Assert.AreEqual("003", table[1].Values[0]);
        }

        [TestMethod]
        public void SimpleSkipAboveTableAmountTest()
        {
            var query = @"select Name from #A.Entities() skip 100";
            var sources = new Dictionary<string, IEnumerable<BasicEntity>>
            {
                {
                    "#A",
                    new[]
                    {
                        new BasicEntity("001"), new BasicEntity("002"), new BasicEntity("003"), new BasicEntity("004"),
                        new BasicEntity("005"), new BasicEntity("006")
                    }
                }
            };

            var vm = CreateAndRunVirtualMachine(query, sources);
            var table = vm.Run();

            Assert.AreEqual(0, table.Count);
        }

        [TestMethod]
        public void SimpleTakeAboveTableAmountTest()
        {
            var query = @"select Name from #A.Entities() take 100";
            var sources = new Dictionary<string, IEnumerable<BasicEntity>>
            {
                {
                    "#A",
                    new[]
                    {
                        new BasicEntity("001"), new BasicEntity("002"), new BasicEntity("003"), new BasicEntity("004"),
                        new BasicEntity("005"), new BasicEntity("006")
                    }
                }
            };

            var vm = CreateAndRunVirtualMachine(query, sources);
            var table = vm.Run();

            Assert.AreEqual(6, table.Count);
            Assert.AreEqual("001", table[0].Values[0]);
            Assert.AreEqual("002", table[1].Values[0]);
            Assert.AreEqual("003", table[2].Values[0]);
            Assert.AreEqual("004", table[3].Values[0]);
            Assert.AreEqual("005", table[4].Values[0]);
            Assert.AreEqual("006", table[5].Values[0]);
        }

        [TestMethod]
        public void SimpleSkipTakeAboveTableAmountTest()
        {
            var query = @"select Name from #A.Entities() skip 100 take 100";
            var sources = new Dictionary<string, IEnumerable<BasicEntity>>
            {
                {
                    "#A",
                    new[]
                    {
                        new BasicEntity("001"), new BasicEntity("002"), new BasicEntity("003"), new BasicEntity("004"),
                        new BasicEntity("005"), new BasicEntity("006")
                    }
                }
            };

            var vm = CreateAndRunVirtualMachine(query, sources);
            var table = vm.Run();

            Assert.AreEqual(0, table.Count);
        }
    }
}
