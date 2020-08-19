using Microsoft.VisualStudio.TestTools.UnitTesting;
using Traficante.TSQL.Tests;

namespace Traficante.TSQL.Evaluator.Tests.Core
{
    [TestClass]
    public class SubQueryTests : TestBase
    {
        [TestMethod]
        public void SelectAll()
        {
            TSQLEngine sut = new TSQLEngine();
            sut.AddTable("Person", new Person[] {
                new Person { Id = 1, FirstName = "John", LastName = "Smith" },
                new Person { Id = 2, FirstName = "Joe", LastName = "Smith" }
            });

            var result = sut.RunAndReturnTable("SELECT * FROM (SELECT * FROM Person) a");
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual("John", result[0][1]);
            Assert.AreEqual("Joe", result[1][1]);
        }

        [TestMethod]
        public void SelectAllWithWhere()
        {
            TSQLEngine sut = new TSQLEngine();
            sut.AddTable("Person", new Person[] {
                new Person { Id = 1, FirstName = "John", LastName = "Smith" },
                new Person { Id = 2, FirstName = "Joe", LastName = "Smith" }
            });

            var result = sut.RunAndReturnTable("SELECT * FROM (SELECT * FROM Person WHERE FirstName = 'Joe') a");
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("Joe", result[0][1]);
        }

        [TestMethod]
        public void SelectOneColumnWithWhere()
        {
            TSQLEngine sut = new TSQLEngine();
            sut.AddTable("Person", new Person[] {
                new Person { Id = 1, FirstName = "John", LastName = "Smith" },
                new Person { Id = 2, FirstName = "Joe", LastName = "Smith" }
            });

            var result = sut.RunAndReturnTable("SELECT * FROM (SELECT FirstName FROM Person WHERE FirstName = 'Joe') a");
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("Joe", result[0][0]);
        }

        [TestMethod]
        public void SelectAllWithWhereAndParentheses()
        {
            TSQLEngine sut = new TSQLEngine();
            sut.AddTable("Person", new Person[] {
                new Person { Id = 1, FirstName = "John", LastName = "Smith" },
                new Person { Id = 2, FirstName = "Joe", LastName = "Smith" }
            });

            var result = sut.RunAndReturnTable("SELECT * FROM (SELECT * FROM Person WHERE ((FirstName = 'Joe') AND (FirstName = 'Joe')) a");
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("Joe", result[0][1]);
        }
    }
}