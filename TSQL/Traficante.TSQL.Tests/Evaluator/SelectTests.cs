using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Traficante.TSQL.Evaluator.Tests.Core.Schema;
using Traficante.TSQL.Tests;

namespace Traficante.TSQL.Evaluator.Tests.Core
{
    [TestClass]
    public class SelectTests : TestBase
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
            Assert.AreEqual(typeof(decimal), table.Columns.ElementAt(0).ColumnType);

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
        public void AddOperatorWithStringsTurnsIntoConcatTest()
        {
            var query = "select 'abc' + 'cda' from #A.Entities()";
            var sources = new Dictionary<string, IEnumerable<BasicEntity>>
            {
                {"#A", new[] {new BasicEntity("ABCAACBA")}}
            };

            var vm = CreateAndRunVirtualMachine(query, sources);
            var table = vm.Run();

            Assert.AreEqual(1, table.Columns.Count());
            Assert.AreEqual("'abc' + 'cda'", table.Columns.ElementAt(0).ColumnName);
            Assert.AreEqual(typeof(string), table.Columns.ElementAt(0).ColumnType);

            Assert.AreEqual(1, table.Count);
            Assert.AreEqual("abccda", table[0].Values[0]);
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
        public void CanPassComplexArgumentToFunctionTest()
        {
            var query = "select NothingToDo(Self) from #A.Entities()";
            var sources = new Dictionary<string, IEnumerable<BasicEntity>>
            {
                {
                    "#A",
                    new[]
                    {
                        new BasicEntity("001")
                        {
                            Name = "ABBA",
                            Country = "POLAND",
                            City = "CRACOV",
                            Money = 1.23m,
                            Month = "JANUARY",
                            Population = 250
                        }
                    }
                }
            };

            var vm = CreateAndRunVirtualMachine(query, sources);
            var table = vm.Run();

            Assert.AreEqual(1, table.Columns.Count());
            Assert.AreEqual("NothingToDo(Self)", table.Columns.ElementAt(0).ColumnName);
            Assert.AreEqual(typeof(BasicEntity), table.Columns.ElementAt(0).ColumnType);

            Assert.AreEqual(1, table.Count);
            Assert.AreEqual(typeof(BasicEntity), table[0].Values[0].GetType());
        }

        [TestMethod]
        public void TableShouldReturnComplexTypeTest()
        {
            var query = "select Self from #A.Entities()";
            var sources = new Dictionary<string, IEnumerable<BasicEntity>>
            {
                {
                    "#A",
                    new[]
                    {
                        new BasicEntity("001")
                        {
                            Name = "ABBA",
                            Country = "POLAND",
                            City = "CRACOV",
                            Money = 1.23m,
                            Month = "JANUARY",
                            Population = 250
                        }
                    }
                }
            };

            var vm = CreateAndRunVirtualMachine(query, sources);
            var table = vm.Run();

            Assert.AreEqual(1, table.Columns.Count());
            Assert.AreEqual("Self", table.Columns.ElementAt(0).ColumnName);
            Assert.AreEqual(typeof(BasicEntity), table.Columns.ElementAt(0).ColumnType);

            Assert.AreEqual(1, table.Count);
            Assert.AreEqual(typeof(BasicEntity), table[0].Values[0].GetType());
        }

        [TestMethod]
        public void SimpleShowAllColumnsTest()
        {
            var entity = new BasicEntity("001")
            {
                Name = "ABBA",
                Country = "POLAND",
                City = "CRACOV",
                Money = 1.23m,
                Month = "JANUARY",
                Population = 250,
                Time = DateTime.MaxValue,
                Id = 5,
                NullableValue = null
            };
            var query = "select 1, *, Name as Name2, ToString(Self) as SelfString from #A.Entities()";
            var sources = new Dictionary<string, IEnumerable<BasicEntity>>
            {
                {"#A", new[] {entity}}
            };

            var vm = CreateAndRunVirtualMachine(query, sources);
            var table = vm.Run();
            Assert.AreEqual("1", table.Columns.ElementAt(0).ColumnName);
            Assert.AreEqual(typeof(int), table.Columns.ElementAt(0).ColumnType);

            Assert.AreEqual("Name", table.Columns.ElementAt(1).ColumnName);
            Assert.AreEqual(typeof(string), table.Columns.ElementAt(1).ColumnType);

            Assert.AreEqual("City", table.Columns.ElementAt(2).ColumnName);
            Assert.AreEqual(typeof(string), table.Columns.ElementAt(2).ColumnType);

            Assert.AreEqual("Country", table.Columns.ElementAt(3).ColumnName);
            Assert.AreEqual(typeof(string), table.Columns.ElementAt(3).ColumnType);

            Assert.AreEqual("Population", table.Columns.ElementAt(4).ColumnName);
            Assert.AreEqual(typeof(decimal), table.Columns.ElementAt(4).ColumnType);

            Assert.AreEqual("Self", table.Columns.ElementAt(5).ColumnName);
            Assert.AreEqual(typeof(BasicEntity), table.Columns.ElementAt(5).ColumnType);

            Assert.AreEqual("Money", table.Columns.ElementAt(6).ColumnName);
            Assert.AreEqual(typeof(decimal), table.Columns.ElementAt(6).ColumnType);

            Assert.AreEqual("Month", table.Columns.ElementAt(7).ColumnName);
            Assert.AreEqual(typeof(string), table.Columns.ElementAt(7).ColumnType);

            Assert.AreEqual("Time", table.Columns.ElementAt(8).ColumnName);
            Assert.AreEqual(typeof(DateTime), table.Columns.ElementAt(8).ColumnType);

            Assert.AreEqual("Id", table.Columns.ElementAt(9).ColumnName);
            Assert.AreEqual(typeof(int), table.Columns.ElementAt(9).ColumnType);

            Assert.AreEqual("NullableValue", table.Columns.ElementAt(10).ColumnName);
            Assert.AreEqual(typeof(int?), table.Columns.ElementAt(10).ColumnType);

            Assert.AreEqual("Array", table.Columns.ElementAt(11).ColumnName);
            Assert.AreEqual(typeof(int[]), table.Columns.ElementAt(11).ColumnType);

            Assert.AreEqual("Name2", table.Columns.ElementAt(12).ColumnName);
            Assert.AreEqual(typeof(string), table.Columns.ElementAt(12).ColumnType);

            Assert.AreEqual("SelfString", table.Columns.ElementAt(13).ColumnName);
            Assert.AreEqual(typeof(string), table.Columns.ElementAt(13).ColumnType);

            Assert.AreEqual(1, table.Count);

            Assert.AreEqual(Convert.ToInt32(1), table[0].Values[0]);
            Assert.AreEqual("ABBA", table[0].Values[1]);
            Assert.AreEqual("CRACOV", table[0].Values[2]);
            Assert.AreEqual("POLAND", table[0].Values[3]);
            Assert.AreEqual(250m, table[0].Values[4]);
            Assert.AreEqual(entity, table[0].Values[5]);
            Assert.AreEqual(1.23m, table[0].Values[6]);
            Assert.AreEqual("JANUARY", table[0].Values[7]);
            Assert.AreEqual(DateTime.MaxValue, table[0].Values[8]);
            Assert.AreEqual(5, table[0].Values[9]);
            Assert.AreEqual(null, table[0].Values[10]);
            Assert.AreEqual("ABBA", table[0].Values[12]);
            Assert.AreEqual("TEST STRING", table[0].Values[13]);
        }

        [TestMethod]
        public void SimpleAccessArrayTest()
        {
            var query = @"select Self.Array[2] from #A.Entities()";
            var sources = new Dictionary<string, IEnumerable<BasicEntity>>
            {
                {"#A", new[] {new BasicEntity("001"), new BasicEntity("002")}}
            };

            var vm = CreateAndRunVirtualMachine(query, sources);
            var table = vm.Run();

            Assert.AreEqual("Self.Array[2]", table.Columns.ElementAt(0).ColumnName);
            Assert.AreEqual(typeof(int), table.Columns.ElementAt(0).ColumnType);

            Assert.AreEqual(2, table.Count);
            Assert.AreEqual(2, table[0].Values[0]);
            Assert.AreEqual(2, table[1].Values[0]);
        }

        [TestMethod]
        public void SimpleAccessObjectTest()
        {
            var query = @"select Self.Array from #A.Entities()";
            var sources = new Dictionary<string, IEnumerable<BasicEntity>>
            {
                {"#A", new[] {new BasicEntity("001")}}
            };

            var vm = CreateAndRunVirtualMachine(query, sources);
            var table = vm.Run();

            Assert.AreEqual("Self.Array", table.Columns.ElementAt(0).ColumnName);
            Assert.AreEqual(typeof(int[]), table.Columns.ElementAt(0).ColumnType);

            Assert.AreEqual(1, table.Count);
        }

        [TestMethod]
        public void AccessObjectTest()
        {
            var query = @"select Self.Self.Array from #A.Entities()";
            var sources = new Dictionary<string, IEnumerable<BasicEntity>>
            {
                {"#A", new[] {new BasicEntity("001")}}
            };

            var vm = CreateAndRunVirtualMachine(query, sources);
            var table = vm.Run();

            Assert.AreEqual("Self.Self.Array", table.Columns.ElementAt(0).ColumnName);
            Assert.AreEqual(typeof(int[]), table.Columns.ElementAt(0).ColumnType);

            Assert.AreEqual(1, table.Count);
        }

        [TestMethod]
        public void SimpleAccessObjectIncrementTest()
        {
            var query = @"select Inc(Self.Array[2]) from #A.Entities()";
            var sources = new Dictionary<string, IEnumerable<BasicEntity>>
            {
                {"#A", new[] {new BasicEntity("001"), new BasicEntity("002")}}
            };

            var vm = CreateAndRunVirtualMachine(query, sources);
            var table = vm.Run();

            Assert.AreEqual("Inc(Self.Array[2])", table.Columns.ElementAt(0).ColumnName);
            Assert.AreEqual(typeof(long), table.Columns.ElementAt(0).ColumnType);

            Assert.AreEqual(2, table.Count);
            Assert.AreEqual(Convert.ToInt64(3), table[0].Values[0]);
            Assert.AreEqual(Convert.ToInt64(3), table[1].Values[0]);
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
        public void SimpleQueryTest()
        {
            var query = @"select Name as 'x1' from #A.Entities()";
            var sources = new Dictionary<string, IEnumerable<BasicEntity>>
            {
                {"#A", new[] {new BasicEntity("001"), new BasicEntity("002")}}
            };

            var vm = CreateAndRunVirtualMachine(query, sources);
            var table = vm.Run();

            Assert.AreEqual(2, table.Count);
            Assert.AreEqual("001", table[0].Values[0]);
            Assert.AreEqual("002", table[1].Values[0]);
        }

        [TestMethod]
        public void IdentifiersInBracketTest()
        {
            var query = @"select [Name] as 'x1' from [#A].Entities()";
            var sources = new Dictionary<string, IEnumerable<BasicEntity>>
            {
                {"#A", new[] {new BasicEntity("001"), new BasicEntity("002")}}
            };

            var vm = CreateAndRunVirtualMachine(query, sources);
            var table = vm.Run();

            Assert.AreEqual(2, table.Count);
            Assert.AreEqual("001", table[0].Values[0]);
            Assert.AreEqual("002", table[1].Values[0]);
        }

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

        [TestMethod]
        public void ColumnNamesSimpleTest()
        {
            var query =
                @"select Name as TestName, GetOne(), GetOne() as TestColumn, GetTwo(4d, 'test') from #A.Entities()";
            var sources = new Dictionary<string, IEnumerable<BasicEntity>>
            {
                {"#A", new BasicEntity[] { }}
            };

            var vm = CreateAndRunVirtualMachine(query, sources);
            var table = vm.Run();

            Assert.AreEqual(4, table.Columns.Count());
            Assert.AreEqual("TestName", table.Columns.ElementAt(0).ColumnName);
            Assert.AreEqual(typeof(string), table.Columns.ElementAt(0).ColumnType);

            Assert.AreEqual("GetOne()", table.Columns.ElementAt(1).ColumnName);
            Assert.AreEqual(typeof(decimal), table.Columns.ElementAt(1).ColumnType);

            Assert.AreEqual("TestColumn", table.Columns.ElementAt(2).ColumnName);
            Assert.AreEqual(typeof(decimal), table.Columns.ElementAt(2).ColumnType);

            Assert.AreEqual("GetTwo(4, 'test')", table.Columns.ElementAt(3).ColumnName);
            Assert.AreEqual(typeof(string), table.Columns.ElementAt(3).ColumnType);
        }

        [TestMethod]
        public void ColumnTypeDateTimeTest()
        {
            var query = "select Time from #A.entities()";

            var sources = new Dictionary<string, IEnumerable<BasicEntity>>
            {
                {
                    "#A", new[]
                    {
                        new BasicEntity(DateTime.MinValue)
                    }
                }
            };

            var vm = CreateAndRunVirtualMachine(query, sources);
            var table = vm.Run();

            Assert.AreEqual(1, table.Columns.Count());
            Assert.AreEqual("Time", table.Columns.ElementAt(0).ColumnName);

            Assert.AreEqual(1, table.Count());
            Assert.AreEqual(DateTime.MinValue, table[0].Values[0]);
        }

        [TestMethod]
        public void SelectDecimalWithoutMarkingNumberExplicitlyTest()
        {
            var query = "select 1.0, -1.0 from #A.entities()";

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

            Assert.AreEqual(2, table.Columns.Count());
            Assert.AreEqual("1.0", table.Columns.ElementAt(0).ColumnName);
            Assert.AreEqual(typeof(decimal), table.Columns.ElementAt(0).ColumnType);
            Assert.AreEqual("-1.0", table.Columns.ElementAt(1).ColumnName);
            Assert.AreEqual(typeof(decimal), table.Columns.ElementAt(1).ColumnType);

            Assert.AreEqual(1, table.Count());
            Assert.AreEqual(1.0m, table[0].Values[0]);
            Assert.AreEqual(-1.0m, table[0].Values[1]);
        }

        [TestMethod]
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

            Assert.IsTrue(table.Any(row => (string) row[0] == "Name" && (string) row[2] == "System.String"));
            Assert.IsTrue(table.Any(row => (string) row[0] == "City" && (string) row[2] == "System.String"));
            Assert.IsTrue(table.Any(row => (string) row[0] == "Country" && (string) row[2] == "System.String"));
            Assert.IsTrue(table.Any(row => (string) row[0] == "Self" && (string) row[2] == "Traficante.TSQL.Evaluator.Tests.Core.Schema.BasicEntity"));
            Assert.IsTrue(table.Any(row => (string) row[0] == "Money" && (string) row[2] == "System.Decimal"));
            Assert.IsTrue(table.Any(row => (string) row[0] == "Month" && (string) row[2] == "System.String"));
            Assert.IsTrue(table.Any(row => (string) row[0] == "Time" && (string) row[2] == "System.DateTime"));
            Assert.IsTrue(table.Any(row => (string) row[0] == "Id" && (string) row[2] == "System.Int32"));
            Assert.IsTrue(table.Any(row => (string) row[0] == "NullableValue" && (string) row[2] == "System.Nullable`1[System.Int32]"));
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

        [TestMethod]
        [Ignore]
        public void AggregateValuesTest()
        {
            var query = @"select AggregateValues(Name) from #A.entities() a group by Name";

            var sources = new Dictionary<string, IEnumerable<BasicEntity>>
            {
                {
                    "#A", new[]
                    {
                        new BasicEntity("A"),
                        new BasicEntity("B")
                    }
                }
            };

            var vm = CreateAndRunVirtualMachine(query, sources);
            var table = vm.Run();


            Assert.AreEqual("A", table[0][0]);
            Assert.AreEqual("B", table[1][0]);
        }

        [TestMethod]
        [Ignore]
        public void AggregateValuesParentTest()
        {
            var query = @"select AggregateValues(Name, 1) from #A.entities() a group by Name";

            var sources = new Dictionary<string, IEnumerable<BasicEntity>>
            {
                {
                    "#A", new[]
                    {
                        new BasicEntity("A"),
                        new BasicEntity("B")
                    }
                }
            };

            var vm = CreateAndRunVirtualMachine(query, sources);
            var table = vm.Run();

            Assert.AreEqual("A,B", table[0][0]);
        }


        [TestMethod]
        public void SubtractTwoAliasedValuesTest()
        {
            var query = "select a.Money - a.Money from #A.entities() a";

            var sources = new Dictionary<string, IEnumerable<BasicEntity>>
            {
                {
                    "#A", new[]
                    {
                        new BasicEntity("may", 2512m)
                    }
                }
            };

            var vm = CreateAndRunVirtualMachine(query, sources);
            var table = vm.Run();

            Assert.AreEqual(0m, table[0][0]);
        }

        [TestMethod]
        public void SubtractThreeAliasedValuesTest()
        {
            var query = "select (a.Money - a.Population) / a.Money from #A.entities() a";

            var sources = new Dictionary<string, IEnumerable<BasicEntity>>
            {
                {
                    "#A", new[]
                    {
                        new BasicEntity("may", 100m) { Population = 10 }
                    }
                }
            };

            var vm = CreateAndRunVirtualMachine(query, sources);
            var table = vm.Run();

            Assert.AreEqual(0.9m, table[0][0]);
        }

        [TestMethod]
        public void FilterByComplexObjectAccessInWhereTest()
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
        public void CaseWhenSimpleTest()
        {
            var query = "select " +
                "   (case " +
                "       when Population > 100d" +
                "       then true" +
                "       else false" +
                "   end)" +
                "from #A.entities()";

            var sources = new Dictionary<string, IEnumerable<BasicEntity>>
            {
                {
                    "#A", new[]
                    {
                        new BasicEntity("may", 100m) { Population = 100 },
                        new BasicEntity("june", 200m) { Population = 200 }
                    }
                }
            };

            var vm = CreateAndRunVirtualMachine(query, sources);
            var table = vm.Run();

            Assert.AreEqual(2, table.Count);

            Assert.AreEqual(false, table[0][0]);
            Assert.AreEqual(true, table[1][0]);
        }

        [TestMethod]
        public void CaseWhenWithLibraryMethodCallTest()
        {
            var query = "select " +
                "   (case " +
                "       when Population > 100d" +
                "       then GetOne()" +
                "       else Inc(GetOne())" +
                "   end)" +
                "from #A.entities() entities";

            var sources = new Dictionary<string, IEnumerable<BasicEntity>>
            {
                {
                    "#A", new[]
                    {
                        new BasicEntity("may", 100m) { Population = 100 },
                        new BasicEntity("june", 200m) { Population = 200 }
                    }
                }
            };

            var vm = CreateAndRunVirtualMachine(query, sources);
            var table = vm.Run();

            Assert.AreEqual(2, table.Count);

            Assert.AreEqual(2m, table[0][0]);
            Assert.AreEqual(1m, table[1][0]);
        }

        [TestMethod]
        public void MultipleCaseWhenWithLibraryMethodCallTest()
        {
            var query = "select " +
                "   (case " +
                "       when Population > 100d" +
                "       then GetOne()" +
                "       else Inc(GetOne())" +
                "   end)," +
                "   (case " +
                "       when Population <= 100d" +
                "       then GetOne()" +
                "       else Inc(GetOne())" +
                "   end)" +
                "from #A.entities() entities";

            var sources = new Dictionary<string, IEnumerable<BasicEntity>>
            {
                {
                    "#A", new[]
                    {
                        new BasicEntity("may", 100m) { Population = 100 },
                        new BasicEntity("june", 200m) { Population = 200 }
                    }
                }
            };

            var vm = CreateAndRunVirtualMachine(query, sources);
            var table = vm.Run();

            Assert.AreEqual(2, table.Count);

            Assert.AreEqual(2m, table[0][0]);
            Assert.AreEqual(1m, table[0][1]);
            Assert.AreEqual(1m, table[1][0]);
            Assert.AreEqual(2m, table[1][1]);
        }

        [TestMethod]
        public void Select_TwoQueries()
        {
            TSQLEngine sut = new TSQLEngine();
            sut.AddTable("Persons", new Person[] {
                new Person { Id = 1, FirstName = "John", LastName = "Smith" },
                new Person { Id = 2, FirstName = "John", LastName = "Doe" },
                new Person { Id = 3, FirstName = "Joe", LastName = "Block" }
            });
            sut.AddTable("Persons2", new Person[] {
                new Person { Id = 5, FirstName = "Daniel", LastName = "Json" },
                new Person { Id = 6, FirstName = "Mark", LastName = "Stanford" },
            });


            var resunt = sut.RunAndReturnTable("SELECT * FROM Persons WHERE FirstName = 'John'; SELECT * FROM Persons2 WHERE FirstName = 'Daniel'");
            Assert.AreEqual(1, resunt.Count);
            Assert.AreEqual(5, resunt[0][0]);
            Assert.AreEqual("Daniel", resunt[0][1]);
            Assert.AreEqual("Json", resunt[0][2]);
        }

        [TestMethod]
        public void Select_QueryAndSelectFunction()
        {
            TSQLEngine sut = new TSQLEngine();
            sut.AddTable("Persons", new Person[] {
                new Person { Id = 1, FirstName = "John", LastName = "Smith" },
                new Person { Id = 2, FirstName = "John", LastName = "Doe" },
                new Person { Id = 2, FirstName = "Joe", LastName = "Block" }
            });

            var resunt = sut.RunAndReturnTable("SELECT * FROM Persons WHERE FirstName = 'John'; SELECT GetDate()");
            Assert.AreEqual(1, resunt.Count);
            Assert.IsTrue(resunt[0][0] is DateTimeOffset);
        }
    }
}