using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Traficante.Connect.Connectors;

namespace Traficante.Connect.Tests
{
    [TestClass]
    public class ConnectEngineTests
    {
        [TestMethod]
        public void Select_From_Csv()
        {
            ConnectEngine connectEngine = new ConnectEngine();
            connectEngine.AddConector(new CsvConnectorConfig { Alias = "csv" });
            var table = connectEngine.RunAndReturnTable("SELECT id,email FROM csv.fromFile('employees.csv')");
            Assert.AreEqual(8, table.Count);
            Assert.AreEqual("27", table[0][0]);
            Assert.AreEqual("andrew@chinookcorp.com", table[0][1]);
            Assert.AreEqual("175", table[1][0]);
            Assert.AreEqual("nancy@chinookcorp.com", table[1][1]);
        }

        [TestMethod]
        public void Select_From_Sqlite()
        {
            ConnectEngine connectEngine = new ConnectEngine();
            connectEngine.AddConector(new SqliteConnectorConfig { Alias = "chinook", Database = "chinook.db" });
            var table = connectEngine.RunAndReturnTable("SELECT EmployeeId, Email FROM chinook.employees");
            Assert.AreEqual(8, table.Count);
            Assert.AreEqual((Int64)1, table[0][0]);
            Assert.AreEqual("andrew@chinookcorp.com", table[0][1]);
            Assert.AreEqual((Int64)2, table[1][0]);
            Assert.AreEqual("nancy@chinookcorp.com", table[1][1]);
        }

        [TestMethod]
        public void Select_From_Csv_Join_Sqlite()
        {
            ConnectEngine connectEngine = new ConnectEngine();
            connectEngine.AddConector(new CsvConnectorConfig { Alias = "csv" });
            connectEngine.AddConector(new SqliteConnectorConfig { Alias = "chinook", Database = "chinook.db" });
            var table = connectEngine.RunAndReturnTable(@"
                SELECT e2.EmployeeId, e1.last_name FROM csv.fromFile('employees.csv') e1
                INNER JOIN chinook.employees e2 ON e2.Email = e1.email
                ");
            Assert.AreEqual(8, table.Count);
            Assert.AreEqual((Int64)1, table[0][0]);
            Assert.AreEqual("Adams", table[0][1]);
            Assert.AreEqual((Int64)2, table[1][0]);
            Assert.AreEqual("Edwards", table[1][1]);
        }
    }
}
