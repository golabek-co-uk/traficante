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
            Assert.AreEqual(typeof(int?), table.Columns.ElementAt(0).ColumnType);

            Assert.AreEqual("Name", table.Columns.ElementAt(1).ColumnName);
            Assert.AreEqual(typeof(string), table.Columns.ElementAt(1).ColumnType);

            Assert.AreEqual("City", table.Columns.ElementAt(2).ColumnName);
            Assert.AreEqual(typeof(string), table.Columns.ElementAt(2).ColumnType);

            Assert.AreEqual("Country", table.Columns.ElementAt(3).ColumnName);
            Assert.AreEqual(typeof(string), table.Columns.ElementAt(3).ColumnType);

            Assert.AreEqual("Population", table.Columns.ElementAt(4).ColumnName);
            Assert.AreEqual(typeof(decimal?), table.Columns.ElementAt(4).ColumnType);

            Assert.AreEqual("Self", table.Columns.ElementAt(5).ColumnName);
            Assert.AreEqual(typeof(BasicEntity), table.Columns.ElementAt(5).ColumnType);

            Assert.AreEqual("Money", table.Columns.ElementAt(6).ColumnName);
            Assert.AreEqual(typeof(decimal?), table.Columns.ElementAt(6).ColumnType);

            Assert.AreEqual("Month", table.Columns.ElementAt(7).ColumnName);
            Assert.AreEqual(typeof(string), table.Columns.ElementAt(7).ColumnType);

            Assert.AreEqual("Time", table.Columns.ElementAt(8).ColumnName);
            Assert.AreEqual(typeof(DateTime?), table.Columns.ElementAt(8).ColumnType);

            Assert.AreEqual("Id", table.Columns.ElementAt(9).ColumnName);
            Assert.AreEqual(typeof(int?), table.Columns.ElementAt(9).ColumnType);

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
            Assert.AreEqual(typeof(int?), table.Columns.ElementAt(0).ColumnType);

            Assert.AreEqual(2, table.Count);
            Assert.AreEqual(2, table[0].Values[0]);
            Assert.AreEqual(2, table[1].Values[0]);
        }

        [TestMethod]
        public void SimpleAccessArrayTest2()
        {
            var query = @"select Array[2] from #A.Entities()";
            var sources = new Dictionary<string, IEnumerable<BasicEntity>>
            {
                {"#A", new[] {new BasicEntity("001"), new BasicEntity("002")}}
            };

            var vm = CreateAndRunVirtualMachine(query, sources);
            var table = vm.Run();

            Assert.AreEqual("Array[2]", table.Columns.ElementAt(0).ColumnName);
            Assert.AreEqual(typeof(int?), table.Columns.ElementAt(0).ColumnType);

            Assert.AreEqual(2, table.Count);
            Assert.AreEqual(2, table[0].Values[0]);
            Assert.AreEqual(2, table[1].Values[0]);
        }

        [TestMethod]
        public void SimpleAccessArrayTest3()
        {
            var query = @"select e.Array[2] from #A.Entities() e";
            var sources = new Dictionary<string, IEnumerable<BasicEntity>>
            {
                {"#A", new[] {new BasicEntity("001"), new BasicEntity("002")}}
            };

            var vm = CreateAndRunVirtualMachine(query, sources);
            var table = vm.Run();

            Assert.AreEqual("e.Array[2]", table.Columns.ElementAt(0).ColumnName);
            Assert.AreEqual(typeof(int?), table.Columns.ElementAt(0).ColumnType);

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
            Assert.AreEqual(typeof(long?), table.Columns.ElementAt(0).ColumnType);

            Assert.AreEqual(2, table.Count);
            Assert.AreEqual(Convert.ToInt64(3), table[0].Values[0]);
            Assert.AreEqual(Convert.ToInt64(3), table[1].Values[0]);
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
            Assert.AreEqual(typeof(decimal?), table.Columns.ElementAt(1).ColumnType);

            Assert.AreEqual("TestColumn", table.Columns.ElementAt(2).ColumnName);
            Assert.AreEqual(typeof(decimal?), table.Columns.ElementAt(2).ColumnType);

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
            Assert.AreEqual(typeof(decimal?), table.Columns.ElementAt(0).ColumnType);
            Assert.AreEqual("-1.0", table.Columns.ElementAt(1).ColumnName);
            Assert.AreEqual(typeof(decimal?), table.Columns.ElementAt(1).ColumnType);

            Assert.AreEqual(1, table.Count());
            Assert.AreEqual(1.0m, table[0].Values[0]);
            Assert.AreEqual(-1.0m, table[0].Values[1]);
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

        [TestMethod]
        public void SelectColumnWithAlias()
        {
            var query = "select a.Money, a.Money AliasedMoney, a.Money [Aliased Money], a.Money AS AsAliasedMoney, a.Money as [As Aliased Money] from #A.entities() a";

            var sources = new Dictionary<string, IEnumerable<BasicEntity>>
            {
                {
                    "#A", new[]
                    {
                        new BasicEntity("may", 100m)
                    }
                }
            };

            var vm = CreateAndRunVirtualMachine(query, sources);
            var table = vm.Run();

            Assert.AreEqual("a.Money", table.Columns.ElementAt(0).ColumnName);
            Assert.AreEqual(100m, table[0][0]);

            Assert.AreEqual("AliasedMoney", table.Columns.ElementAt(1).ColumnName);
            Assert.AreEqual(100m, table[0][1]);

            Assert.AreEqual("Aliased Money", table.Columns.ElementAt(2).ColumnName);
            Assert.AreEqual(100m, table[0][2]);

            Assert.AreEqual("AsAliasedMoney", table.Columns.ElementAt(3).ColumnName);
            Assert.AreEqual(100m, table[0][2]);

            Assert.AreEqual("As Aliased Money", table.Columns.ElementAt(4).ColumnName);
            Assert.AreEqual(100m, table[0][2]);

        }

        [TestMethod]
        public void SelectTableWithAlias()
        {
            TSQLEngine sut = new TSQLEngine();
            sut.AddTable("Persons", new Person[] {
                new Person { Id = 1, FirstName = "John", LastName = "Smith" }
            });

            Assert.AreEqual("John", sut.RunAndReturnTable("SELECT p.FirstName FROM Persons p")[0][0]);
            Assert.AreEqual("John", sut.RunAndReturnTable("SELECT p.FirstName FROM Persons [p]")[0][0]);
            Assert.AreEqual("John", sut.RunAndReturnTable("SELECT [p p].FirstName FROM Persons [p p]")[0][0]);
            Assert.AreEqual("John", sut.RunAndReturnTable("SELECT [p p].FirstName FROM Persons AS [p p]")[0][0]);
        }

        [TestMethod]
        public void SelectTableWithReservedWord()
        {
            TSQLEngine sut = new TSQLEngine();
            sut.AddTable("SELECT", new Person[] {
                new Person { Id = 1, FirstName = "John", LastName = "Smith" }
            });
            sut.AddTable("FROM", new Person[] {
                new Person { Id = 1, FirstName = "John", LastName = "Smith" }
            });


            Assert.AreEqual("John", sut.RunAndReturnTable("SELECT p.FirstName FROM [SELECT] p")[0][0]);
            Assert.AreEqual("John", sut.RunAndReturnTable("SELECT p.FirstName FROM [FROM] p")[0][0]);
        }

    }
}