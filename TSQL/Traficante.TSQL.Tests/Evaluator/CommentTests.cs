using Microsoft.VisualStudio.TestTools.UnitTesting;
using Traficante.TSQL.Tests;

namespace Traficante.TSQL.Evaluator.Tests.Core
{
    [TestClass]
    public class CommentTests : TestBase
    {
        [TestMethod]
        public void CommentOutEndOfOneLineQuery()
        {
            TSQLEngine sut = new TSQLEngine();
            sut.AddTable("Persons", new Person[] {
                new Person { Id = 1, FirstName = "John", LastName = "Smith" }
            });

            var resunt = sut.RunAndReturnTable("SELECT FirstName FROM Persons --some comment");

            Assert.AreEqual(1, resunt.Count);
            Assert.AreEqual("John", resunt[0][0]);
        }

        [TestMethod]
        public void CommentOutFirstLineAndLastLine()
        {
            TSQLEngine sut = new TSQLEngine();
            sut.AddTable("Persons", new Person[] {
                new Person { Id = 1, FirstName = "John", LastName = "Smith" }
            });

            var resunt = sut.RunAndReturnTable("--some comment\nSELECT FirstName FROM Persons \n--some comment");

            Assert.AreEqual(1, resunt.Count);
            Assert.AreEqual("John", resunt[0][0]);
        }

        [TestMethod]
        public void CommentOutEndOfFirstLineIneTheQuery()
        {
            TSQLEngine sut = new TSQLEngine();
            sut.AddTable("Persons", new Person[] {
                new Person { Id = 1, FirstName = "John", LastName = "Smith" }
            });

            var resunt = sut.RunAndReturnTable("SELECT FirstName --some comment\nFROM Persons ");

            Assert.AreEqual(1, resunt.Count);
            Assert.AreEqual("John", resunt[0][0]);
        }
    }
}