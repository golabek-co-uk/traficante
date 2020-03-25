using CsvHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace Traficante.TSQL.Tests
{
    [TestClass]
    public class EngineTests
    {
        

        [TestMethod]
        public void Select_All_From_TableInBrackets()
        {
            TSQLEngine sut = new TSQLEngine();
            sut.AddTable("55dog-animals", new Person[] {
                new Person { Id = 1, FirstName = "John", LastName = "Smith" },
                new Person { Id = 2, FirstName = "John", LastName = "Doe" },
                new Person { Id = 2, FirstName = "Joe", LastName = "Block" }
            });

            var resunt = sut.RunAndReturnTable("SELECT * FROM [55dog-animals] WHERE FirstName = 'John'");
            Assert.AreEqual(2, resunt.Count);
        }

        [TestMethod]
        public void Select_All_From_Where()
        {
            TSQLEngine sut = new TSQLEngine();
            sut.AddTable("Persons", new Person[] {
                new Person { Id = 1, FirstName = "John", LastName = "Smith" },
                new Person { Id = 2, FirstName = "John", LastName = "Doe" },
                new Person { Id = 2, FirstName = "Joe", LastName = "Block" }
            });

            var resunt = sut.RunAndReturnTable("SELECT * FROM Persons WHERE FirstName = 'John'");
            Assert.AreEqual(2, resunt.Count);
            Assert.AreEqual(1, resunt[0][0]);
            Assert.AreEqual("John", resunt[0][1]);
            Assert.AreEqual("Smith", resunt[0][2]);
            Assert.AreEqual(2, resunt[1][0]);
            Assert.AreEqual("John", resunt[1][1]);
            Assert.AreEqual("Doe", resunt[1][2]);
        }

        [TestMethod]
        public void Select_All_FromWithAlias_Where()
        {
            TSQLEngine sut = new TSQLEngine();
            sut.AddTable("Persons", new Person[] {
                new Person { Id = 1, FirstName = "John", LastName = "Smith" },
                new Person { Id = 2, FirstName = "John", LastName = "Doe" },
                new Person { Id = 2, FirstName = "Joe", LastName = "Block" }
            });

            var resunt = sut.RunAndReturnTable("SELECT * FROM Persons p WHERE FirstName = 'John'");
            Assert.AreEqual(2, resunt.Count);
            Assert.AreEqual(1, resunt[0][0]);
            Assert.AreEqual("John", resunt[0][1]);
            Assert.AreEqual("Smith", resunt[0][2]);
            Assert.AreEqual(2, resunt[1][0]);
            Assert.AreEqual("John", resunt[1][1]);
            Assert.AreEqual("Doe", resunt[1][2]);
        }

        [TestMethod]
        public void Select_ColumnWithoutAlias_FromWithAlias_Where()
        {
            TSQLEngine sut = new TSQLEngine();
            sut.AddTable("Persons", new Person[] {
                new Person { Id = 1, FirstName = "John", LastName = "Smith" },
                new Person { Id = 2, FirstName = "John", LastName = "Doe" },
                new Person { Id = 2, FirstName = "Joe", LastName = "Block" }
            });

            var resunt = sut.RunAndReturnTable("SELECT LastName FROM Persons p WHERE FirstName = 'John'");
            Assert.AreEqual(2, resunt.Count);
            Assert.AreEqual("Smith", resunt[0][0]);
            Assert.AreEqual("Doe", resunt[1][0]);
        }

        [TestMethod]
        public void Select_ColumnWithAlias_FromWithAlias_Where()
        {
            TSQLEngine sut = new TSQLEngine();
            sut.AddTable("Persons", new Person[] {
                new Person { Id = 1, FirstName = "John", LastName = "Smith" },
                new Person { Id = 2, FirstName = "John", LastName = "Doe" },
                new Person { Id = 2, FirstName = "Joe", LastName = "Block" }
            });

            var resunt = sut.RunAndReturnTable("SELECT p.LastName FROM Persons p WHERE p.FirstName = 'John'");
            Assert.AreEqual(2, resunt.Count);
            Assert.AreEqual("Smith", resunt[0][0]);
            Assert.AreEqual("Doe", resunt[1][0]);
        }

        [TestMethod]
        public void Select_ColumnWithTableName_From_Where()
        {
            TSQLEngine sut = new TSQLEngine();
            sut.AddTable("Persons", new Person[] {
                new Person { Id = 1, FirstName = "John", LastName = "Smith" },
                new Person { Id = 2, FirstName = "John", LastName = "Doe" },
                new Person { Id = 2, FirstName = "Joe", LastName = "Block" }
            });

            var resunt = sut.RunAndReturnTable("SELECT Persons.LastName FROM Persons WHERE Persons.FirstName = 'John'");
            Assert.AreEqual(2, resunt.Count);
            Assert.AreEqual("Smith", resunt[0][0]);
            Assert.AreEqual("Doe", resunt[1][0]);
        }

        [TestMethod]
        public void SelectFrom_DefaultSchema_Table()
        {
            TSQLEngine sut = new TSQLEngine();
            sut.AddTable("Persons", new string[1] { "dbo" }, new Person[] {
                new Person { Id = 1, FirstName = "John", LastName = "Smith" },
                new Person { Id = 2, FirstName = "John", LastName = "Doe" },
                new Person { Id = 2, FirstName = "Joe", LastName = "Block" }
            });

            var resunt = sut.RunAndReturnTable("SELECT * FROM Persons WHERE FirstName = 'John'");
            Assert.AreEqual(2, resunt.Count);
            Assert.AreEqual(1, resunt[0][0]);
            Assert.AreEqual("John", resunt[0][1]);
            Assert.AreEqual("Smith", resunt[0][2]);
            Assert.AreEqual(2, resunt[1][0]);
            Assert.AreEqual("John", resunt[1][1]);
            Assert.AreEqual("Doe", resunt[1][2]);
        }

        [TestMethod]
        public void SelectFrom_DefaultDatabase_DefaultSchema_Table()
        {
            TSQLEngine sut = new TSQLEngine();
            sut.AddTable("Persons", new string[2] { "master", "dbo" }, new Person[] {
                new Person { Id = 1, FirstName = "John", LastName = "Smith" },
                new Person { Id = 2, FirstName = "John", LastName = "Doe" },
                new Person { Id = 2, FirstName = "Joe", LastName = "Block" }
            });

            var resunt = sut.RunAndReturnTable("SELECT * FROM Persons WHERE FirstName = 'John'");
            Assert.AreEqual(2, resunt.Count);
            Assert.AreEqual(1, resunt[0][0]);
            Assert.AreEqual("John", resunt[0][1]);
            Assert.AreEqual("Smith", resunt[0][2]);
            Assert.AreEqual(2, resunt[1][0]);
            Assert.AreEqual("John", resunt[1][1]);
            Assert.AreEqual("Doe", resunt[1][2]);
        }

        [TestMethod]
        public void SelectFrom_CustomSchema_Table()
        {
            TSQLEngine sut = new TSQLEngine();
            sut.AddTable("Persons", new string[1] { "xxx" }, new Person[] {
                new Person { Id = 1, FirstName = "John", LastName = "Smith" },
                new Person { Id = 2, FirstName = "John", LastName = "Doe" },
                new Person { Id = 2, FirstName = "Joe", LastName = "Block" }
            });

            var resunt = sut.RunAndReturnTable("SELECT * FROM xxx.Persons WHERE FirstName = 'John'");
            Assert.AreEqual(2, resunt.Count);
            Assert.AreEqual(1, resunt[0][0]);
            Assert.AreEqual("John", resunt[0][1]);
            Assert.AreEqual("Smith", resunt[0][2]);
            Assert.AreEqual(2, resunt[1][0]);
            Assert.AreEqual("John", resunt[1][1]);
            Assert.AreEqual("Doe", resunt[1][2]);
        }

        [TestMethod]
        public void SelectFrom_CustomtDatabase_CustomSchema_Table()
        {
            TSQLEngine sut = new TSQLEngine();
            sut.AddTable("Persons", new string[2] { "yyy", "xxx" }, new Person[] {
                new Person { Id = 1, FirstName = "John", LastName = "Smith" },
                new Person { Id = 2, FirstName = "John", LastName = "Doe" },
                new Person { Id = 2, FirstName = "Joe", LastName = "Block" }
            });

            var resunt = sut.RunAndReturnTable("SELECT * FROM yyy.xxx.Persons WHERE FirstName = 'John'");
            Assert.AreEqual(2, resunt.Count);
            Assert.AreEqual(1, resunt[0][0]);
            Assert.AreEqual("John", resunt[0][1]);
            Assert.AreEqual("Smith", resunt[0][2]);
            Assert.AreEqual(2, resunt[1][0]);
            Assert.AreEqual("John", resunt[1][1]);
            Assert.AreEqual("Doe", resunt[1][2]);
        }

        [TestMethod]
        public void SelectFrom_CustomServer_CustomtDatabase_CustomSchema_Table()
        {
            TSQLEngine sut = new TSQLEngine();
            sut.AddTable("Persons", new string[3] { "zzz","yyy", "xxx" }, new Person[] {
                new Person { Id = 1, FirstName = "John", LastName = "Smith" },
                new Person { Id = 2, FirstName = "John", LastName = "Doe" },
                new Person { Id = 2, FirstName = "Joe", LastName = "Block" }
            });

            var resunt = sut.RunAndReturnTable("SELECT * FROM zzz.yyy.xxx.Persons WHERE FirstName = 'John'");
            Assert.AreEqual(2, resunt.Count);
            Assert.AreEqual(1, resunt[0][0]);
            Assert.AreEqual("John", resunt[0][1]);
            Assert.AreEqual("Smith", resunt[0][2]);
            Assert.AreEqual(2, resunt[1][0]);
            Assert.AreEqual("John", resunt[1][1]);
            Assert.AreEqual("Doe", resunt[1][2]);
        }

        [TestMethod]
        public void SelectFrom_DefaultDatabase_CustomSchema_Table()
        {
            TSQLEngine sut = new TSQLEngine();
            sut.AddTable("Persons", new string[2] { "hr", "dbo" }, new Person[] {
                new Person { Id = 1, FirstName = "John", LastName = "Smith" },
                new Person { Id = 2, FirstName = "John", LastName = "Doe" },
                new Person { Id = 2, FirstName = "Joe", LastName = "Block" }
            });

            var resunt = sut.RunAndReturnTable("SELECT * FROM dbo.Persons WHERE FirstName = 'John'");
            Assert.AreEqual(2, resunt.Count);
            Assert.AreEqual(1, resunt[0][0]);
            Assert.AreEqual("John", resunt[0][1]);
            Assert.AreEqual("Smith", resunt[0][2]);
            Assert.AreEqual(2, resunt[1][0]);
            Assert.AreEqual("John", resunt[1][1]);
            Assert.AreEqual("Doe", resunt[1][2]);
        }

        [TestMethod]
        public void SelectFrom_IDataReader_Table()
        {
            using (TSQLEngine sut = new TSQLEngine())
            {
                sut.AddTable("Persons", new string[0] { }, () =>
                 {
                     var reader = new StreamReader("csv.csv");
                     var csvReader = new CsvReader(reader, CultureInfo.InvariantCulture, false);
                     return new CsvDataReader(csvReader);
                 });

                var table = sut.RunAndReturnTable("SELECT * FROM Persons");
                Assert.AreEqual(2, table.Count);
                Assert.AreEqual("1", table[0][0]);
                Assert.AreEqual("one", table[0][1]);
                Assert.AreEqual("2", table[1][0]);
                Assert.AreEqual("two", table[1][1]);
            }
        }

        [TestMethod]
        public void SelectFrom_Resolver_IDataReader_Table()
        {
            using (TSQLEngine sut = new TSQLEngine())
            {
                sut.AddTableResolver((name, path) =>
                {
                    if (name == "Persons")
                    {
                        Func<CsvDataReader> @delegate = () =>
                        {
                            var reader = new StreamReader("csv.csv");
                            var csvReader = new CsvReader(reader, CultureInfo.InvariantCulture, false);
                            return new CsvDataReader(csvReader);
                        };
                        return @delegate;
                    }
                    return null;
                });

                var table = sut.RunAndReturnTable("SELECT * FROM Persons");
                Assert.AreEqual(2, table.Count);
                Assert.AreEqual("1", table[0][0]);
                Assert.AreEqual("one", table[0][1]);
                Assert.AreEqual("2", table[1][0]);
                Assert.AreEqual("two", table[1][1]);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(TSQLException))]
        public void SelectFrom_ColumnDoesNotExist()
        {
            TSQLEngine sut = new TSQLEngine();
            sut.AddTable("Persons", new Person[] {
                new Person { Id = 1, FirstName = "John", LastName = "Smith" }
            });

            var resunt = sut.RunAndReturnTable("SELECT NotExistingColumn FROM Persons");
        }

        [TestMethod]
        [ExpectedException(typeof(TSQLException))]
        public void SelectFrom_NotExistingTable()
        {
            TSQLEngine sut = new TSQLEngine();
            var resunt = sut.RunAndReturnTable("SELECT * FROM NotExistingTable");
        }

        [TestMethod]
        [ExpectedException(typeof(TSQLException))]
        public void Select_FunctionDoesNotExist()
        {
            TSQLEngine sut = new TSQLEngine();
            sut.AddTable("Persons", new Person[] {
                new Person { Id = 1, FirstName = "John", LastName = "Smith" }
            });

            var resunt = sut.RunAndReturnTable("SELECT SomeFunction(FirstName) FROM Persons");
        }

    }

    public class Person
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}
