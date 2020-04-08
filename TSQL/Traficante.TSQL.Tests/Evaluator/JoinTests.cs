using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Traficante.TSQL.Evaluator.Tests.Core.Schema;
using Traficante.TSQL.Tests;

namespace Traficante.TSQL.Evaluator.Tests.Core
{
    [TestClass]
    public class JoinTests : TestBase
    {
        [TestMethod]
        public void SimpleOneJoinTest()
        {
            var query = "select countries.Country, cities.City from #A.entities() countries inner join #B.entities() cities on countries.Country = cities.Country";

            var sources = new Dictionary<string, IEnumerable<BasicEntity>>
            {
                {
                    "#A", new[]
                    {
                        new BasicEntity("Poland", null),
                        new BasicEntity("Germany", null)
                    }
                },
                {
                    "#B", new[]
                    {
                        new BasicEntity("Poland", "Krakow"),
                        new BasicEntity("Poland", "Wroclaw"),
                        new BasicEntity("Poland", "Warszawa"),
                        new BasicEntity("Poland", "Gdansk"),
                        new BasicEntity("Germany", "Berlin"),
                        new BasicEntity("Czechia", "Prague")
                    }
                }
            };

            var vm = CreateAndRunVirtualMachine(query, sources);
            var table = vm.Run();

            Assert.AreEqual(2, table.Columns.Count());
            Assert.AreEqual(5, table.Rows.Count());

            Assert.AreEqual("countries.Country", table.Columns.ElementAt(0).ColumnName);
            Assert.AreEqual(typeof(string), table.Columns.ElementAt(0).ColumnType);
            Assert.AreEqual(0, table.Columns.ElementAt(0).ColumnIndex);

            Assert.AreEqual("cities.City", table.Columns.ElementAt(1).ColumnName);
            Assert.AreEqual(typeof(string), table.Columns.ElementAt(1).ColumnType);
            Assert.AreEqual(1, table.Columns.ElementAt(1).ColumnIndex);

            AreEquivalent(new [] {
                    new object[] { "Poland", "Krakow" },
                    new object[] { "Poland", "Wroclaw" },
                    new object[] { "Poland", "Warszawa" },
                    new object[] { "Poland", "Gdansk" },
                    new object[] { "Germany", "Berlin" }
                }, table);
        }

        public void AssertContain(DataTable table, object[] values)
        {
            foreach(var row in table.Rows)
            {
                if (row.Values.Length != values.Length)
                {
                    throw new Exception($"Row at index {table.Rows.IndexOf(row)} has array with {row.Values.Length} values. Expected {values.Length} values.");
                }
                bool equal = true;
                for(int i = 0; i < values.Length; i++)
                {
                    if (row.Values[i] == default && values[i] != default)
                        continue;

                    if (row.Values[i]?.ToString() == values[i]?.ToString())
                        continue;
                        
                    equal = false;
                }
                if (equal)
                    return;
            }
            throw new Exception($"Table does not contain expected values.");
        }

        [TestMethod]
        public void SimpleTwoJoinsTest()
        {
            var query =
                "select countries.Country, cities.City, population.Population from #A.entities() countries inner join #B.entities() cities on countries.Country = cities.Country inner join #C.entities() population on cities.City = population.City";

            var sources = new Dictionary<string, IEnumerable<BasicEntity>>
            {
                {
                    "#A", new[]
                    {
                        new BasicEntity("Poland", "Krakow"),
                        new BasicEntity("Germany", "Berlin")
                    }
                },
                {
                    "#B", new[]
                    {
                        new BasicEntity("Poland", "Krakow"),
                        new BasicEntity("Poland", "Wroclaw"),
                        new BasicEntity("Poland", "Warszawa"),
                        new BasicEntity("Poland", "Gdansk"),
                        new BasicEntity("Germany", "Berlin")
                    }
                },
                {
                    "#C", new[]
                    {
                        new BasicEntity {City = "Krakow", Population = 400},
                        new BasicEntity {City = "Wroclaw", Population = 500},
                        new BasicEntity {City = "Warszawa", Population = 1000},
                        new BasicEntity {City = "Gdansk", Population = 200},
                        new BasicEntity {City = "Berlin", Population = 400}
                    }
                }
            };

            var vm = CreateAndRunVirtualMachine(query, sources);
            var table = vm.Run();

            Assert.AreEqual(3, table.Columns.Count());

            Assert.AreEqual("countries.Country", table.Columns.ElementAt(0).ColumnName);
            Assert.AreEqual(typeof(string), table.Columns.ElementAt(0).ColumnType);
            Assert.AreEqual(0, table.Columns.ElementAt(0).ColumnIndex);

            Assert.AreEqual("cities.City", table.Columns.ElementAt(1).ColumnName);
            Assert.AreEqual(typeof(string), table.Columns.ElementAt(1).ColumnType);
            Assert.AreEqual(1, table.Columns.ElementAt(1).ColumnIndex);

            Assert.AreEqual("population.Population", table.Columns.ElementAt(2).ColumnName);
            Assert.AreEqual(typeof(decimal), table.Columns.ElementAt(2).ColumnType);
            Assert.AreEqual(2, table.Columns.ElementAt(2).ColumnIndex);

            AreEquivalent(
                new[]
                {
                    new object[] { "Poland", "Krakow", (decimal)400 },
                    new object[] { "Poland", "Wroclaw", (decimal)500 },
                    new object[] { "Poland", "Warszawa", (decimal)1000 },
                    new object[] { "Poland", "Gdansk", (decimal)200 },
                    new object[] { "Germany", "Berlin", (decimal)400 }
                },
                table);

        }

        [TestMethod]
        public void JoinWithCaseWhen2Test()
        {
            var query = "select countries.Country, (case when population.Population > 400 then ToUpperInvariant(cities.City) else cities.City end) as 'cities.City', population.Population from #A.entities() countries inner join #B.entities() cities on countries.Country = cities.Country inner join #C.entities() population on cities.City = population.City";

            var sources = new Dictionary<string, IEnumerable<BasicEntity>>
            {
                {
                    "#A", new[]
                    {
                        new BasicEntity("Poland", "Krakow"),
                        new BasicEntity("Germany", "Berlin")
                    }
                },
                {
                    "#B", new[]
                    {
                        new BasicEntity("Poland", "Krakow"),
                        new BasicEntity("Poland", "Wroclaw"),
                        new BasicEntity("Poland", "Warszawa"),
                        new BasicEntity("Poland", "Gdansk"),
                        new BasicEntity("Germany", "Berlin")
                    }
                },
                {
                    "#C", new[]
                    {
                        new BasicEntity {City = "Krakow", Population = 400},
                        new BasicEntity {City = "Wroclaw", Population = 500},
                        new BasicEntity {City = "Warszawa", Population = 1000},
                        new BasicEntity {City = "Gdansk", Population = 200},
                        new BasicEntity {City = "Berlin", Population = 400}
                    }
                }
            };

            var vm = CreateAndRunVirtualMachine(query, sources);
            var table = vm.Run();

            Assert.AreEqual(3, table.Columns.Count());

            Assert.AreEqual("countries.Country", table.Columns.ElementAt(0).ColumnName);
            Assert.AreEqual(typeof(string), table.Columns.ElementAt(0).ColumnType);
            Assert.AreEqual(0, table.Columns.ElementAt(0).ColumnIndex);

            Assert.AreEqual("cities.City", table.Columns.ElementAt(1).ColumnName);
            Assert.AreEqual(typeof(string), table.Columns.ElementAt(1).ColumnType);
            Assert.AreEqual(1, table.Columns.ElementAt(1).ColumnIndex);

            Assert.AreEqual("population.Population", table.Columns.ElementAt(2).ColumnName);
            Assert.AreEqual(typeof(decimal), table.Columns.ElementAt(2).ColumnType);
            Assert.AreEqual(2, table.Columns.ElementAt(2).ColumnIndex);

            AreEquivalent(new[] {
                    new object[] { "Poland", "Krakow", 400m },
                    new object[] { "Poland", "WROCLAW", 500m },
                    new object[] { "Poland", "WARSZAWA",  1000m },
                    new object[] { "Poland", "Gdansk", 200m },
                    new object[] { "Germany", "Berlin", 400m },
                    },
                table);
        }

        [TestMethod]
        public void JoinWithCaseWhenTest()
        {
            var query =
                "select countries.Country, (case when population.Population >= 500 then 'big' else 'low' end), population.Population from #A.entities() countries inner join #B.entities() cities on countries.Country = cities.Country inner join #C.entities() population on cities.City = population.City";

            var sources = new Dictionary<string, IEnumerable<BasicEntity>>
            {
                {
                    "#A", new[]
                    {
                        new BasicEntity("Poland", "Krakow"),
                        new BasicEntity("Germany", "Berlin")
                    }
                },
                {
                    "#B", new[]
                    {
                        new BasicEntity("Poland", "Krakow"),
                        new BasicEntity("Poland", "Wroclaw"),
                        new BasicEntity("Poland", "Warszawa"),
                        new BasicEntity("Poland", "Gdansk"),
                        new BasicEntity("Germany", "Berlin")
                    }
                },
                {
                    "#C", new[]
                    {
                        new BasicEntity {City = "Krakow", Population = 400},
                        new BasicEntity {City = "Wroclaw", Population = 500},
                        new BasicEntity {City = "Warszawa", Population = 1000},
                        new BasicEntity {City = "Gdansk", Population = 200},
                        new BasicEntity {City = "Berlin", Population = 400}
                    }
                }
            };

            var vm = CreateAndRunVirtualMachine(query, sources);
            var table = vm.Run();

            Assert.AreEqual(3, table.Columns.Count());

            Assert.AreEqual("countries.Country", table.Columns.ElementAt(0).ColumnName);
            Assert.AreEqual(typeof(string), table.Columns.ElementAt(0).ColumnType);
            Assert.AreEqual(0, table.Columns.ElementAt(0).ColumnIndex);

            Assert.AreEqual("case when population.Population >= 500 then 'big' else 'low' end", table.Columns.ElementAt(1).ColumnName);
            Assert.AreEqual(typeof(string), table.Columns.ElementAt(1).ColumnType);
            Assert.AreEqual(1, table.Columns.ElementAt(1).ColumnIndex);

            Assert.AreEqual("population.Population", table.Columns.ElementAt(2).ColumnName);
            Assert.AreEqual(typeof(decimal), table.Columns.ElementAt(2).ColumnType);
            Assert.AreEqual(2, table.Columns.ElementAt(2).ColumnIndex);

            AreEquivalent(new[] {
                    new object[] { "Poland", "low", 400m},
                    new object[] { "Poland", "big", 500m },
                    new object[] { "Poland", "big", 1000m },
                    new object[] { "Poland", "low", 200m },
                    new object[] { "Germany", "low", 400m},
                    },
            table);
        }

        [TestMethod]
        public void JoinWithGroupByTest()
        {
            var query =
                "select cities.Country, Sum(population.Population) from #A.entities() countries inner join #B.entities() cities on countries.Country = cities.Country inner join #C.entities() population on cities.City = population.City group by cities.Country";

            var sources = new Dictionary<string, IEnumerable<BasicEntity>>
            {
                {
                    "#A", new[]
                    {
                        new BasicEntity("Poland", "Krakow"),
                        new BasicEntity("Germany", "Berlin")
                    }
                },
                {
                    "#B", new[]
                    {
                        new BasicEntity("Poland", "Krakow"),
                        new BasicEntity("Poland", "Wroclaw"),
                        new BasicEntity("Poland", "Warszawa"),
                        new BasicEntity("Poland", "Gdansk"),
                        new BasicEntity("Germany", "Berlin")
                    }
                },
                {
                    "#C", new[]
                    {
                        new BasicEntity {City = "Krakow", Population = 400},
                        new BasicEntity {City = "Wroclaw", Population = 500},
                        new BasicEntity {City = "Warszawa", Population = 1000},
                        new BasicEntity {City = "Gdansk", Population = 200},
                        new BasicEntity {City = "Berlin", Population = 400}
                    }
                }
            };

            var vm = CreateAndRunVirtualMachine(query, sources);
            var table = vm.Run();

            Assert.AreEqual(2, table.Columns.Count());

            Assert.AreEqual("cities.Country", table.Columns.ElementAt(0).ColumnName);
            Assert.AreEqual(typeof(string), table.Columns.ElementAt(0).ColumnType);
            Assert.AreEqual(0, table.Columns.ElementAt(0).ColumnIndex);

            Assert.AreEqual("Sum(population.Population)", table.Columns.ElementAt(1).ColumnName);
            Assert.AreEqual(typeof(decimal), table.Columns.ElementAt(1).ColumnType);
            Assert.AreEqual(1, table.Columns.ElementAt(1).ColumnIndex);

            AreEquivalent(new[] {
                    new object[] { "Poland",  2100m },
                    new object[] { "Germany",  400m },
                    },
            table);
        }

        [TestMethod]
        public void JoinWithExceptTest()
        {
            var query =
       @"
select 
    countries.Country, cities.City, population.Population 
from #A.entities() countries 
inner join #B.entities() cities on countries.Country = cities.Country 
inner join #C.entities() population on cities.City = population.City 
except (countries.Country, cities.City, population.Population)
select 
    countries.Country, cities.City, population.Population 
from #A.entities() countries 
inner join #B.entities() cities on countries.Country = cities.Country 
inner join #C.entities() population on cities.City = population.City";

            var sources = new Dictionary<string, IEnumerable<BasicEntity>>
            {
                {
                    "#A", new[]
                    {
                        new BasicEntity("Poland", "Krakow"),
                        new BasicEntity("Germany", "Berlin")
                    }
                },
                {
                    "#B", new[]
                    {
                        new BasicEntity("Poland", "Krakow"),
                        new BasicEntity("Poland", "Wroclaw"),
                        new BasicEntity("Poland", "Warszawa"),
                        new BasicEntity("Poland", "Gdansk"),
                        new BasicEntity("Germany", "Berlin")
                    }
                },
                {
                    "#C", new[]
                    {
                        new BasicEntity {City = "Krakow", Population = 400},
                        new BasicEntity {City = "Wroclaw", Population = 500},
                        new BasicEntity {City = "Warszawa", Population = 1000},
                        new BasicEntity {City = "Gdansk", Population = 200},
                        new BasicEntity {City = "Berlin", Population = 400}
                    }
                }
            };

            var vm = CreateAndRunVirtualMachine(query, sources);
            var table = vm.Run();

            Assert.AreEqual(3, table.Columns.Count());

            Assert.AreEqual("countries.Country", table.Columns.ElementAt(0).ColumnName);
            Assert.AreEqual(typeof(string), table.Columns.ElementAt(0).ColumnType);
            Assert.AreEqual(0, table.Columns.ElementAt(0).ColumnIndex);

            Assert.AreEqual("cities.City", table.Columns.ElementAt(1).ColumnName);
            Assert.AreEqual(typeof(string), table.Columns.ElementAt(1).ColumnType);
            Assert.AreEqual(1, table.Columns.ElementAt(1).ColumnIndex);

            Assert.AreEqual("population.Population", table.Columns.ElementAt(2).ColumnName);
            Assert.AreEqual(typeof(decimal), table.Columns.ElementAt(2).ColumnType);
            Assert.AreEqual(2, table.Columns.ElementAt(2).ColumnIndex);

            Assert.AreEqual(0, table.Count);
        }

        [TestMethod]
        public void JoinWithUnionTest()
        {
            var query =
                @"
select 
    countries.Country, cities.City, population.Population 
from #A.entities() countries 
inner join #B.entities() cities on countries.Country = cities.Country 
inner join #C.entities() population on cities.City = population.City 
union (countries.Country, cities.City, population.Population)
select 
    countries.Country, cities.City, population.Population 
from #A.entities() countries 
inner join #B.entities() cities on countries.Country = cities.Country 
inner join #C.entities() population on cities.City = population.City";

            var sources = new Dictionary<string, IEnumerable<BasicEntity>>
            {
                {
                    "#A", new[]
                    {
                        new BasicEntity("Poland", "Krakow"),
                        new BasicEntity("Germany", "Berlin")
                    }
                },
                {
                    "#B", new[]
                    {
                        new BasicEntity("Poland", "Krakow"),
                        new BasicEntity("Poland", "Wroclaw"),
                        new BasicEntity("Poland", "Warszawa"),
                        new BasicEntity("Poland", "Gdansk"),
                        new BasicEntity("Germany", "Berlin")
                    }
                },
                {
                    "#C", new[]
                    {
                        new BasicEntity {City = "Krakow", Population = 400},
                        new BasicEntity {City = "Wroclaw", Population = 500},
                        new BasicEntity {City = "Warszawa", Population = 1000},
                        new BasicEntity {City = "Gdansk", Population = 200},
                        new BasicEntity {City = "Berlin", Population = 400}
                    }
                }
            };

            var vm = CreateAndRunVirtualMachine(query, sources);
            var table = vm.Run();

            Assert.AreEqual(3, table.Columns.Count());

            Assert.AreEqual("countries.Country", table.Columns.ElementAt(0).ColumnName);
            Assert.AreEqual(typeof(string), table.Columns.ElementAt(0).ColumnType);
            Assert.AreEqual(0, table.Columns.ElementAt(0).ColumnIndex);

            Assert.AreEqual("cities.City", table.Columns.ElementAt(1).ColumnName);
            Assert.AreEqual(typeof(string), table.Columns.ElementAt(1).ColumnType);
            Assert.AreEqual(1, table.Columns.ElementAt(1).ColumnIndex);

            Assert.AreEqual("population.Population", table.Columns.ElementAt(2).ColumnName);
            Assert.AreEqual(typeof(decimal), table.Columns.ElementAt(2).ColumnType);
            Assert.AreEqual(2, table.Columns.ElementAt(2).ColumnIndex);

            AreEquivalent(new[] {
                    new object[] { "Poland", "Krakow", 400m },
                    new object[] { "Poland", "Wroclaw", 500m },
                    new object[] { "Poland", "Warszawa", 1000m },
                    new object[] { "Poland", "Gdansk", 200m },
                    new object[] { "Germany", "Berlin", 400m },
                    },
                    table);
                

        }


        [TestMethod]
        public void JoinWithUnionAllTest()
        {
            var query =
                @"
select 
    countries.Country, cities.City, population.Population 
from #A.entities() countries 
inner join #B.entities() cities on countries.Country = cities.Country 
inner join #C.entities() population on cities.City = population.City 
union all (countries.Country, cities.City, population.Population)
select 
    countries.Country, cities.City, population.Population 
from #A.entities() countries 
inner join #B.entities() cities on countries.Country = cities.Country 
inner join #C.entities() population on cities.City = population.City";

            var sources = new Dictionary<string, IEnumerable<BasicEntity>>
            {
                {
                    "#A", new[]
                    {
                        new BasicEntity("Poland", "Krakow"),
                        new BasicEntity("Germany", "Berlin")
                    }
                },
                {
                    "#B", new[]
                    {
                        new BasicEntity("Poland", "Krakow"),
                        new BasicEntity("Poland", "Wroclaw"),
                        new BasicEntity("Poland", "Warszawa"),
                        new BasicEntity("Poland", "Gdansk"),
                        new BasicEntity("Germany", "Berlin")
                    }
                },
                {
                    "#C", new[]
                    {
                        new BasicEntity {City = "Krakow", Population = 400},
                        new BasicEntity {City = "Wroclaw", Population = 500},
                        new BasicEntity {City = "Warszawa", Population = 1000},
                        new BasicEntity {City = "Gdansk", Population = 200},
                        new BasicEntity {City = "Berlin", Population = 400}
                    }
                }
            };

            var vm = CreateAndRunVirtualMachine(query, sources);
            var table = vm.Run();

            Assert.AreEqual(3, table.Columns.Count());

            Assert.AreEqual("countries.Country", table.Columns.ElementAt(0).ColumnName);
            Assert.AreEqual(typeof(string), table.Columns.ElementAt(0).ColumnType);
            Assert.AreEqual(0, table.Columns.ElementAt(0).ColumnIndex);

            Assert.AreEqual("cities.City", table.Columns.ElementAt(1).ColumnName);
            Assert.AreEqual(typeof(string), table.Columns.ElementAt(1).ColumnType);
            Assert.AreEqual(1, table.Columns.ElementAt(1).ColumnIndex);

            Assert.AreEqual("population.Population", table.Columns.ElementAt(2).ColumnName);
            Assert.AreEqual(typeof(decimal), table.Columns.ElementAt(2).ColumnType);
            Assert.AreEqual(2, table.Columns.ElementAt(2).ColumnIndex);

            Assert.AreEqual(10, table.Count);

            AreEquivalent(new[] {
                new object[] { "Poland", "Krakow", 400m },
                new object[] { "Poland", "Wroclaw", 500m },
                new object[] { "Poland", "Warszawa", 1000m },
                new object[] { "Poland", "Gdansk", 200m },
                new object[] { "Germany", "Berlin", 400m },
                new object[] { "Poland", "Krakow", 400m },
                new object[] { "Poland", "Wroclaw", 500m },
                new object[] { "Poland", "Warszawa", 1000m },
                new object[] { "Poland", "Gdansk", 200m },
                new object[] { "Germany", "Berlin", 400m },
                    },
                table);

        }

        [TestMethod]
        public void InnerJoinCteTablesTest()
        {
            var query = @"
with p as (
    select Country, City, Id from #A.entities()
), x as (
    select Country, City, Id from #B.entities()
)
select p.Id, x.Id from p inner join x on p.Country = x.Country";

            var sources = new Dictionary<string, IEnumerable<BasicEntity>>
            {
                {
                    "#A", new[]
                    {
                        new BasicEntity("Poland", "Krakow") {Id = 0},
                        new BasicEntity("Germany", "Berlin") {Id = 1},
                        new BasicEntity("Russia", "Moscow") {Id = 2}
                    }
                },
                {
                    "#B", new[]
                    {
                        new BasicEntity("Poland", "Krakow") {Id = 0},
                        new BasicEntity("Poland", "Wroclaw") {Id = 1},
                        new BasicEntity("Poland", "Warszawa") {Id = 2},
                        new BasicEntity("Poland", "Gdansk") {Id = 3},
                        new BasicEntity("Germany", "Berlin") {Id = 4}
                    }
                }
            };

            var vm = CreateAndRunVirtualMachine(query, sources);
            var table = vm.Run();

            Assert.AreEqual(5, table.Count);
            Assert.AreEqual(2, table.Columns.Count());

            AreEquivalent(new[] {
                    new object[] { 0, 0 },
                    new object[] { 0, 1 },
                    new object[] { 0, 2 },
                    new object[] { 0, 3 },
                    new object[] { 1, 4 },
                    },
            table);
        }

        [TestMethod]
        public void InnerJoinCteSelfJoinTest()
        {
            var query = @"
with p as (
    select Country, City, Id from #A.entities()
), x as (
    select Country, City, Id from p
)
select p.Id, x.Id from p inner join x on p.Country = x.Country";

            var sources = new Dictionary<string, IEnumerable<BasicEntity>>
            {
                {
                    "#A", new[]
                    {
                        new BasicEntity("Poland", "Krakow") {Id = 0},
                        new BasicEntity("Germany", "Berlin") {Id = 1},
                        new BasicEntity("Russia", "Moscow") {Id = 2}
                    }
                }
            };

            var vm = CreateAndRunVirtualMachine(query, sources);
            var table = vm.Run();

            Assert.AreEqual(3, table.Count);
            Assert.AreEqual(2, table.Columns.Count());

            AreEquivalent(new[] {
                    new object[] { 0, 0 },
                    new object[] { 1, 1 },
                    new object[] { 2, 2 }
                    },
            table);
        }

        [TestMethod]
        public void ComplexCteIssue1Test()
        {
            var query = @"
with p as (
	select 
        Country
	from #A.entities()
), x as (
	select 
		Country
	from p group by Country 
)
select p.Country, x.Country from p inner join x on p.Country = x.Country where p.Country = 'Poland'
";

            var sources = new Dictionary<string, IEnumerable<BasicEntity>>
            {
                {
                    "#A", new[]
                    {
                        new BasicEntity("Poland", "Krakow") {Id = 0},
                        new BasicEntity("Germany", "Berlin") {Id = 1},
                        new BasicEntity("Russia", "Moscow") {Id = 2}
                    }
                }
            };

            var vm = CreateAndRunVirtualMachine(query, sources);
            var table = vm.Run();

            Assert.AreEqual(1, table.Count);
            Assert.AreEqual(2, table.Columns.Count());

            AreEquivalent(new[] {
                    new object[] { "Poland", "Poland" }
                    },
                table);
        }

        [TestMethod]
        public void ComplexCteIssue1WithGroupByTest()
        {
            var query = @"
with p as (
	select 
        Country
	from #A.entities()
), x as (
	select 
		Country
	from p group by Country 
)
select p.Country, Count(p.Country) from p inner join x on p.Country = x.Country group by p.Country having Count(p.Country) > 1
";

            var sources = new Dictionary<string, IEnumerable<BasicEntity>>
            {
                {
                    "#A", new[]
                    {
                        new BasicEntity("Poland", "Krakow") {Id = 0},
                        new BasicEntity("Poland", "Krakow") {Id = 0},
                        new BasicEntity("Germany", "Berlin") {Id = 1},
                        new BasicEntity("Russia", "Moscow") {Id = 2}
                    }
                }
            };

            var vm = CreateAndRunVirtualMachine(query, sources);
            var table = vm.Run();

            Assert.AreEqual(1, table.Count);
            Assert.AreEqual(2, table.Columns.Count());

            AreEquivalent(new[] {
                new object[] { "Poland", 2 }
            },
            table);
            //Assert.AreEqual("Poland", table[0][0]);
            //Assert.AreEqual(2, table[0][1]);
        }

        [TestMethod]
        public void SimpleLeftJoinTest()
        {
            var query = @"
select a.Id, b.Id from #A.entities() a 
left outer join #B.entities() b on a.Id = b.Id";

            var sources = new Dictionary<string, IEnumerable<BasicEntity>>
            {
                {
                    "#A", new[]
                    {
                        new BasicEntity("Poland", "Krakow") {Id = 0},
                        new BasicEntity("Germany", "Berlin") {Id = 1},
                        new BasicEntity("Russia", "Moscow") {Id = 2}
                    }
                },
                {
                    "#B", new[]
                    {
                        new BasicEntity("Poland", "Krakow") {Id = 0}
                    }
                }
            };

            var vm = CreateAndRunVirtualMachine(query, sources);
            var table = vm.Run();

            Assert.AreEqual(2, table.Columns.Count());
            Assert.AreEqual(3, table.Count);
        }

        [TestMethod]
        public void ThreeTablesLeftJoinTest()
        {
            var query = @"
select a.Id, b.Id, c.Name from #A.entities() a 
left outer join #B.entities() b on a.Id = b.Id
left outer join #C.entities() c on b.Id = c.Id";

            var sources = new Dictionary<string, IEnumerable<BasicEntity>>
            {
                {
                    "#A", new[]
                    {
                        new BasicEntity("Poland", "Krakow") {Id = 0},
                        new BasicEntity("Germany", "Berlin") {Id = 1},
                        new BasicEntity("Russia", "Moscow") {Id = 2}
                    }
                },
                {
                    "#B", new[]
                    {
                        new BasicEntity("Poland", "Krakow") {Id = 0}
                    }
                }
                ,
                {
                    "#C", new BasicEntity[]
                    {
                    }
                }
            };

            var vm = CreateAndRunVirtualMachine(query, sources);
            var table = vm.Run();

            Assert.AreEqual(3, table.Columns.Count());
            Assert.AreEqual(3, table.Count);
        }
    }
}
