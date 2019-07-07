using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

namespace Traficante.TSQL.Tests
{
    [TestClass]
    public class EngineTests
    {
        [TestMethod]
        public void SelectFromTable()
        {
            Engine sut = new Engine();
            sut.AddTable("Persons", new Person[] {
                new Person { Id = 1, FirstName = "John", LastName = "Smith" },
                new Person { Id = 2, FirstName = "John", LastName = "Doe" },
                new Person { Id = 2, FirstName = "Joe", LastName = "Block" }
            });

            var resunt = sut.Run("SELECT * FROM Persons WHERE FirstName = 'John'");
            Assert.AreEqual(2, resunt.Count);
            Assert.AreEqual(1, resunt[0][0]);
            Assert.AreEqual("John", resunt[0][1]);
            Assert.AreEqual("Smith", resunt[0][2]);
            Assert.AreEqual(2, resunt[1][0]);
            Assert.AreEqual("John", resunt[1][1]);
            Assert.AreEqual("Doe", resunt[1][2]);
        }

        [TestMethod]
        public void SelectFromDefaultSchemaAndTable()
        {
            Engine sut = new Engine();
            sut.AddTable("Persons", new Person[] {
                new Person { Id = 1, FirstName = "John", LastName = "Smith" },
                new Person { Id = 2, FirstName = "John", LastName = "Doe" },
                new Person { Id = 2, FirstName = "Joe", LastName = "Block" }
            });

            var resunt = sut.Run("SELECT * FROM dbo.Persons WHERE FirstName = 'John'");
            Assert.AreEqual(2, resunt.Count);
            Assert.AreEqual(1, resunt[0][0]);
            Assert.AreEqual("John", resunt[0][1]);
            Assert.AreEqual("Smith", resunt[0][2]);
            Assert.AreEqual(2, resunt[1][0]);
            Assert.AreEqual("John", resunt[1][1]);
            Assert.AreEqual("Doe", resunt[1][2]);
        }

        [TestMethod]
        public void SelectFromCustomSchemaAndTable()
        {
            Engine sut = new Engine();
            sut.AddTable(null, "xxx", "Persons", new Person[] {
                new Person { Id = 1, FirstName = "John", LastName = "Smith" },
                new Person { Id = 2, FirstName = "John", LastName = "Doe" },
                new Person { Id = 2, FirstName = "Joe", LastName = "Block" }
            });

            var resunt = sut.Run("SELECT * FROM xxx.Persons WHERE FirstName = 'John'");
            Assert.AreEqual(2, resunt.Count);
            Assert.AreEqual(1, resunt[0][0]);
            Assert.AreEqual("John", resunt[0][1]);
            Assert.AreEqual("Smith", resunt[0][2]);
            Assert.AreEqual(2, resunt[1][0]);
            Assert.AreEqual("John", resunt[1][1]);
            Assert.AreEqual("Doe", resunt[1][2]);
        }
    }

    public class Person
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}
