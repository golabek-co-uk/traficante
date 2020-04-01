using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
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
            var result = connectEngine.Run("SELECT id,email FROM csv.fromFile('employees.csv')").Cast<object>().ToList();
            var itemType = result[0].GetType();
            Assert.AreEqual(8, result.Count);
            Assert.AreEqual("27", itemType.GetField("id").GetValue(result[0]));
            Assert.AreEqual("andrew@chinookcorp.com", itemType.GetField("email").GetValue(result[0]));
            Assert.AreEqual("175", itemType.GetField("id").GetValue(result[1]));
            Assert.AreEqual("nancy@chinookcorp.com", itemType.GetField("email").GetValue(result[1]));
        }

        [TestMethod]
        public void Select_From_Sqlite()
        {
            ConnectEngine connectEngine = new ConnectEngine();
            connectEngine.AddConector(new SqliteConnectorConfig { Alias = "chinook", Database = "chinook.db" });
            var result = connectEngine.Run("SELECT EmployeeId, Email FROM chinook.employees").Cast<object>().ToList();
            var itemType = result[0].GetType();
            Assert.AreEqual(8, result.Count);
            Assert.AreEqual((Int64)1, itemType.GetField("EmployeeId").GetValue(result[0]));
            Assert.AreEqual("andrew@chinookcorp.com", itemType.GetField("Email").GetValue(result[0]));
            Assert.AreEqual((Int64)2, itemType.GetField("EmployeeId").GetValue(result[1]));
            Assert.AreEqual("nancy@chinookcorp.com", itemType.GetField("Email").GetValue(result[1]));
        }

        [TestMethod]
        public void Select_From_Csv_Join_Sqlite()
        {
            ConnectEngine connectEngine = new ConnectEngine();
            connectEngine.AddConector(new CsvConnectorConfig { Alias = "csv" });
            connectEngine.AddConector(new SqliteConnectorConfig { Alias = "chinook", Database = "chinook.db" });
            var result = connectEngine.Run(@"
                SELECT e2.EmployeeId, e1.last_name FROM csv.fromFile('employees.csv') e1
                INNER JOIN chinook.employees e2 ON e2.Email = e1.email
                ").Cast<object>().ToList();
            var itemType = result[0].GetType();
            Assert.AreEqual(8, result.Count);
            Assert.AreEqual((Int64)1, itemType.GetField("e2.EmployeeId").GetValue(result[0]));
            Assert.AreEqual("Adams", itemType.GetField("e1.last_name").GetValue(result[0]));
            Assert.AreEqual((Int64)2, itemType.GetField("e2.EmployeeId").GetValue(result[1]));
            Assert.AreEqual("Edwards", itemType.GetField("e1.last_name").GetValue(result[1]));
        }
    }
}
