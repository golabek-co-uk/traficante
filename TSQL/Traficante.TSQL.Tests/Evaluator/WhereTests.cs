using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Traficante.TSQL.Evaluator.Tests.Core.Schema;
using Traficante.TSQL.Tests;

namespace Traficante.TSQL.Evaluator.Tests.Core
{
    [TestClass]
    public class WhereTests : TestBase
    {
        [TestMethod]
        public void ComplexWhere1Test()
        {
            var query =
                $"select Population from #A.Entities() where Population > 0 and Population - 100 > -1.5d and Population - 100 < 1.5d";

            var sources = new Dictionary<string, IEnumerable<BasicEntity>>
            {
                {
                    "#A", new[]
                    {
                        new BasicEntity("WARSAW", "POLAND", 500),
                        new BasicEntity("CZESTOCHOWA", "POLAND", 99),
                        new BasicEntity("KATOWICE", "POLAND", 101),
                        new BasicEntity("BERLIN", "GERMANY", 50)
                    }
                }
            };

            var vm = CreateAndRunVirtualMachine(query, sources);
            var table = vm.Run();

            Assert.AreEqual(1, table.Columns.Count());
            Assert.AreEqual("Population", table.Columns.ElementAt(0).ColumnName);
            Assert.AreEqual(typeof(decimal?), table.Columns.ElementAt(0).ColumnType);

            Assert.AreEqual(2, table.Count);
            Assert.AreEqual(99m, table[0].Values[0]);
            Assert.AreEqual(101m, table[1].Values[0]);
        }

        [TestMethod]
        public void MultipleAndOperatorTest()
        {
            var query =
                "select Name from #A.Entities() where IndexOf(Name, 'A') = 0 and IndexOf(Name, 'B') = 1 and IndexOf(Name, 'C') = 2";
            var sources = new Dictionary<string, IEnumerable<BasicEntity>>
            {
                {
                    "#A",
                    new[] {new BasicEntity("A"), new BasicEntity("AB"), new BasicEntity("ABC"), new BasicEntity("ABCD")}
                }
            };

            var vm = CreateAndRunVirtualMachine(query, sources);
            var table = vm.Run();

            Assert.AreEqual(1, table.Columns.Count());
            Assert.AreEqual("Name", table.Columns.ElementAt(0).ColumnName);
            Assert.AreEqual(typeof(string), table.Columns.ElementAt(0).ColumnType);

            Assert.AreEqual(2, table.Count);
            Assert.AreEqual("ABC", table[0].Values[0]);
            Assert.AreEqual("ABCD", table[1].Values[0]);
        }

        [TestMethod]
        public void MultipleOrOperatorTest()
        {
            var query = "select Name from #A.Entities() where Name = 'ABC' or Name = 'ABCD' or Name = 'A'";
            var sources = new Dictionary<string, IEnumerable<BasicEntity>>
            {
                {
                    "#A",
                    new[] {new BasicEntity("A"), new BasicEntity("AB"), new BasicEntity("ABC"), new BasicEntity("ABCD")}
                }
            };

            var vm = CreateAndRunVirtualMachine(query, sources);
            var table = vm.Run();

            Assert.AreEqual(1, table.Columns.Count());
            Assert.AreEqual("Name", table.Columns.ElementAt(0).ColumnName);
            Assert.AreEqual(typeof(string), table.Columns.ElementAt(0).ColumnType);

            Assert.AreEqual(3, table.Count);
            Assert.AreEqual("A", table[0].Values[0]);
            Assert.AreEqual("ABC", table[1].Values[0]);
            Assert.AreEqual("ABCD", table[2].Values[0]);
        }

        [TestMethod]
        public void WhereWithOrTest()
        {
            var query = @"select Name from #A.Entities() where Name = '001' or Name = '005'";
            var sources = new Dictionary<string, IEnumerable<BasicEntity>>
            {
                {"#A", new[] {new BasicEntity("001"), new BasicEntity("002"), new BasicEntity("005")}}
            };

            var vm = CreateAndRunVirtualMachine(query, sources);
            var table = vm.Run();

            Assert.AreEqual(2, table.Count);
            Assert.AreEqual("001", table[0].Values[0]);
            Assert.AreEqual("005", table[1].Values[0]);
        }

        [TestMethod]
        public void ComplexObjectAccessTest()
        {
            var query = "select Population from #A.entities() where Self.Money > 100";

            var sources = new Dictionary<string, IEnumerable<BasicEntity>>
            {
                {
                    "#A", new[]
                    {
                        new BasicEntity("may", 100m) { Population = 10 },
                        new BasicEntity("june", 200m) { Population = 20 }
                    }
                }
            };

            var vm = CreateAndRunVirtualMachine(query, sources);
            var table = vm.Run();

            Assert.AreEqual(1, table.Count);
            Assert.AreEqual(20m, table[0][0]);
        }

        [TestMethod]
        public void ContainsStringsTest()
        {
            var query = "select Name from #A.Entities() where Name contains ('ABC', 'CdA', 'CDA', 'DDABC')";
            var sources = new Dictionary<string, IEnumerable<BasicEntity>>
            {
                {
                    "#A",
                    new[]
                    {
                        new BasicEntity("ABC"),
                        new BasicEntity("XXX"),
                        new BasicEntity("CDA"),
                        new BasicEntity("DDABC")
                    }
                }
            };

            var vm = CreateAndRunVirtualMachine(query, sources);
            var table = vm.Run();

            Assert.AreEqual(1, table.Columns.Count());
            Assert.AreEqual("Name", table.Columns.ElementAt(0).ColumnName);
            Assert.AreEqual(typeof(string), table.Columns.ElementAt(0).ColumnType);

            Assert.AreEqual(3, table.Count);
            Assert.AreEqual("ABC", table[0].Values[0]);
            Assert.AreEqual("CDA", table[1].Values[0]);
            Assert.AreEqual("DDABC", table[2].Values[0]);
        }

        [TestMethod]
        public void CompareIntAndString_AutoConverToInt()
        {
            TSQLEngine sut = new TSQLEngine();
            sut.AddTable("Person", new Person[] {
                new Person { Id = 1, FirstName = "John", LastName = "Smith" },
                new Person { Id = 2, FirstName = "02", LastName = "Smith" }
            });

            var result = sut.RunAndReturnTable("SELECT FirstName FROM Person WHERE  Id = FirstName");
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("02", result[0][0]);
        }

        [TestMethod]
        public void CompareDateAndString()
        {
            TSQLEngine sut = new TSQLEngine();
            sut.AddTable("Person", new Person[] {
                new Person { Id = 1, FirstName = "John", LastName = "Smith", HiredDate = new DateTimeOffset(2020, 5, 6, 10, 9, 0, TimeSpan.Zero) },
                new Person { Id = 2, FirstName = "John", LastName = "Perez", HiredDate = new DateTimeOffset(2020, 5, 6, 11, 9, 0, TimeSpan.Zero) }
            });

            var result = sut.RunAndReturnTable("SELECT LastName FROM Person WHERE HiredDate = '2020-05-06 10:09'");
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("Smith", result[0][0]);
        }

    }
}