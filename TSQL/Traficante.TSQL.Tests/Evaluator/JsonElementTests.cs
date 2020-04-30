using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using Traficante.TSQL.Evaluator.Tests.Core;
using Traficante.TSQL.Evaluator.Tests.Core.Schema;

namespace Traficante.TSQL.Tests.Evaluator
{
    [TestClass]
    public class JsonElementTests : TestBase
    {

        [TestMethod]
        public void Select_JsonColumn()
        {
            TSQLEngine sut = new TSQLEngine();
            sut.AddTable("Persons", new Person[] {
                new Person
                {
                    Id = 1,
                    FirstName = "John",
                    LastName = "Smith",
                    Json = JsonDocument.Parse("{ \"name\":\"John\", \"age\":\"23\", \"address\": { \"city\":\"London\", \"street\":\"Moorgate Street\" }}").RootElement
                }
            });

            var resunt = sut.RunAndReturnTable("SELECT Id, Json FROM Persons");
            Assert.AreEqual(1, resunt.Count);
            Assert.AreEqual(1, resunt[0][0]);
            Assert.AreEqual("{ \"name\":\"John\", \"age\":\"23\", \"address\": { \"city\":\"London\", \"street\":\"Moorgate Street\" }}", resunt[0][1].ToString());
        }

        [TestMethod]
        public void Select_InnerPropertyOfJsonColumn()
        {
            TSQLEngine sut = new TSQLEngine();
            sut.AddTable("Persons", new Person[] {
                new Person
                {
                    Id = 1,
                    FirstName = "John",
                    LastName = "Smith",
                    Json = JsonDocument.Parse("{ \"name\":\"John\", \"age\":\"23\", \"address\": { \"city\":\"London\", \"street\":\"Moorgate Street\" }}").RootElement
                }
            });

            var resunt = sut.RunAndReturnTable("SELECT Id, Json.age FROM Persons");
            Assert.AreEqual(1, resunt.Count);
            Assert.AreEqual(1, resunt[0][0]);
            Assert.AreEqual("23", resunt[0][1].ToString());
        }

        [TestMethod]
        public void Select_InnerInnerPropertyOfJsonColumn()
        {
            TSQLEngine sut = new TSQLEngine();
            sut.AddTable("Persons", new Person[] {
                new Person
                {
                    Id = 1,
                    FirstName = "John",
                    LastName = "Smith",
                    Json = JsonDocument.Parse("{ \"name\":\"John\", \"age\":\"23\", \"address\": { \"city\":\"London\", \"street\":\"Moorgate Street\" }}").RootElement
                }
            });

            var resunt = sut.RunAndReturnTable("SELECT Id, Json.address.city FROM Persons");
            Assert.AreEqual(1, resunt.Count);
            Assert.AreEqual(1, resunt[0][0]);
            Assert.AreEqual("London", resunt[0][1].ToString());
        }

        [TestMethod]
        public void SelectFrom_PropertyDoesNotExist()
        {
            TSQLEngine sut = new TSQLEngine();
            sut.AddTable("Persons", new Person[] {
                new Person
                {
                    Id = 1,
                    FirstName = "John",
                    LastName = "Smith",
                    Json = JsonDocument.Parse("{ \"name\":\"John\", \"age\":\"23\", \"address\": { \"city\":\"London\", \"street\":\"Moorgate Street\" }}").RootElement
                }
            });

            var resunt = sut.RunAndReturnTable("SELECT Id, Json.job.title FROM Persons");
            Assert.AreEqual(1, resunt.Count);
            Assert.AreEqual(1, resunt[0][0]);
            Assert.AreEqual(null, resunt[0][1]);
        }

        [TestMethod]
        public void Select_WhereWithProperty()
        {
            TSQLEngine sut = new TSQLEngine();
            sut.AddTable("Persons", new Person[] {
                new Person
                {
                    Id = 1,
                    FirstName = "John",
                    LastName = "Smith",
                    Json = JsonDocument.Parse("{ \"name\":\"John\", \"age\":\"23\", \"address\": { \"city\":\"London\", \"street\":\"Moorgate Street\" }}").RootElement
                },
                new Person
                {
                    Id = 2,
                    FirstName = "Stive",
                    LastName = "Smith",
                    Json = JsonDocument.Parse("{ \"age\":\"32\", \"address\": { \"city\":\"London\", \"street\":\"Moorgate Street\" }}").RootElement
                },
                new Person
                {
                    Id = 3,
                    FirstName = "Tom",
                    LastName = "Smith",
                    Json = JsonDocument.Parse("{ \"name\":\"Tom\", \"age\":\"37\", \"address\": { \"city\":\"London\", \"street\":\"Moorgate Street\" }}").RootElement
                }
            });

            var resunt = sut.RunAndReturnTable("SELECT Id, Json.age FROM Persons WHERE Json.name = 'John'");
            Assert.AreEqual(1, resunt.Count);
            Assert.AreEqual(1, resunt[0][0]);
            Assert.AreEqual("23", resunt[0][1].ToString());
        }

        [TestMethod]
        public void Select_WhereWithIntProperty()
        {
            TSQLEngine sut = new TSQLEngine();
            sut.AddTable("Persons", new Person[] {
                new Person
                {
                    Id = 1,
                    FirstName = "John",
                    LastName = "Smith",
                    Json = JsonDocument.Parse("{ \"name\":\"John\", \"age\":23, \"address\": { \"city\":\"London\", \"street\":\"Moorgate Street\" }}").RootElement
                },
                new Person
                {
                    Id = 2,
                    FirstName = "Stive",
                    LastName = "Smith",
                    Json = JsonDocument.Parse("{  \"name\":\"Stive\", \"address\": { \"city\":\"London\", \"street\":\"Moorgate Street\" }}").RootElement
                },
                new Person
                {
                    Id = 3,
                    FirstName = "Tom",
                    LastName = "Smith",
                    Json = JsonDocument.Parse("{ \"name\":\"Tom\", \"age\":37, \"address\": { \"city\":\"London\", \"street\":\"Moorgate Street\" }}").RootElement
                }
                ,
                new Person
                {
                    Id = 4,
                    FirstName = "Joe",
                    LastName = "Smith",
                    Json = JsonDocument.Parse("{ \"name\":\"Joe\", \"age\":\"37\", \"address\": { \"city\":\"London\", \"street\":\"Moorgate Street\" }}").RootElement
                }
                 ,
                new Person
                {
                    Id = 5,
                    FirstName = "Dan",
                    LastName = "Smith",
                    Json = JsonDocument.Parse("{ \"name\":\"Dan\", \"age\":\"asdf\", \"address\": { \"city\":\"London\", \"street\":\"Moorgate Street\" }}").RootElement
                }
            });

            var resunt = sut.RunAndReturnTable("SELECT Id, Json.name FROM Persons WHERE Json.age = 37");
            Assert.AreEqual(2, resunt.Count);
            Assert.AreEqual(3, resunt[0][0]);
            Assert.AreEqual("Tom", resunt[0][1].ToString());
            Assert.AreEqual(4, resunt[1][0]);
            Assert.AreEqual("Joe", resunt[1][1].ToString());
        }

        [TestMethod]
        public void Select_OrderByInt()
        {
            TSQLEngine sut = new TSQLEngine();
            sut.AddTable("Persons", new Person[] {
                new Person
                {
                    Id = 1,
                    FirstName = "John",
                    LastName = "Smith",
                    Json = JsonDocument.Parse("{ \"name\":\"John\", \"age\":23, \"address\": { \"city\":\"London\", \"street\":\"Moorgate Street\" }}").RootElement
                },
                new Person
                {
                    Id = 2,
                    FirstName = "Stive",
                    LastName = "Smith",
                    Json = JsonDocument.Parse("{  \"name\":\"Stive\", \"address\": { \"city\":\"London\", \"street\":\"Moorgate Street\" }}").RootElement
                },
                new Person
                {
                    Id = 3,
                    FirstName = "Tom",
                    LastName = "Smith",
                    Json = JsonDocument.Parse("{ \"name\":\"Tom\", \"age\":25, \"address\": { \"city\":\"London\", \"street\":\"Moorgate Street\" }}").RootElement
                }
                ,
                new Person
                {
                    Id = 4,
                    FirstName = "Joe",
                    LastName = "Smith",
                    Json = JsonDocument.Parse("{ \"name\":\"Joe\", \"age\":\"24\", \"address\": { \"city\":\"London\", \"street\":\"Moorgate Street\" }}").RootElement
                }
                 ,
                new Person
                {
                    Id = 5,
                    FirstName = "Dan",
                    LastName = "Smith",
                    Json = JsonDocument.Parse("{ \"name\":\"Dan\", \"age\":\"29\", \"address\": { \"city\":\"London\", \"street\":\"Moorgate Street\" }}").RootElement
                }
            });

            var resunt = sut.RunAndReturnTable("SELECT Id, Json.name FROM Persons ORDER BY Json.age desc");
            Assert.AreEqual(5, resunt.Count);
            Assert.AreEqual("Dan", resunt[0][1].ToString());
            Assert.AreEqual("Tom", resunt[1][1].ToString());
            Assert.AreEqual("Joe", resunt[2][1].ToString());
            Assert.AreEqual("John", resunt[3][1].ToString());
            Assert.AreEqual("Stive", resunt[4][1].ToString());
        }
    }

    public class Person
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public JsonElement Json { get; set; }
    }
}
