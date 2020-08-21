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
            connectEngine.AddConector(new FilesConnectorConfig { Alias = "csv" });
            var result = connectEngine.Run("SELECT id,email FROM csv.fromFile('employees.csv')").Cast<object>().ToList();
            var itemType = result[0].GetType();
            Assert.AreEqual(8, result.Count);
            Assert.AreEqual("27", result[0].GetValue<string>("id"));
            Assert.AreEqual("andrew@chinookcorp.com", result[0].GetValue<string>("email"));
            Assert.AreEqual("175", result[1].GetValue<string>("id"));
            Assert.AreEqual("nancy@chinookcorp.com", result[1].GetValue<string>("email"));
        }

        [TestMethod]
        public void Select_From_Sqlite()
        {
            ConnectEngine connectEngine = new ConnectEngine();
            connectEngine.AddConector(new SqliteConnectorConfig { Alias = "chinook", Database = "chinook.db" });
            var result = connectEngine.Run("SELECT EmployeeId, Email FROM chinook.employees").Cast<object>().ToList();
            var itemType = result[0].GetType();
            Assert.AreEqual(8, result.Count);
            Assert.AreEqual(1, result[0].GetValue<int>("EmployeeId"));
            Assert.AreEqual("andrew@chinookcorp.com", result[0].GetValue<string>("Email"));
            Assert.AreEqual(2, result[1].GetValue<int>("EmployeeId"));
            Assert.AreEqual("nancy@chinookcorp.com", result[1].GetValue<string>("Email"));
        }

        [TestMethod]
        public void Select_From_Csv_Join_Sqlite()
        {
            ConnectEngine connectEngine = new ConnectEngine();
            connectEngine.AddConector(new FilesConnectorConfig { Alias = "csv" });
            connectEngine.AddConector(new SqliteConnectorConfig { Alias = "chinook", Database = "chinook.db" });
            var result = connectEngine.Run(@"
                SELECT e2.EmployeeId, e1.last_name FROM csv.fromFile('employees.csv') e1
                INNER JOIN chinook.employees e2 ON e1.Email = e2.email
                ").Cast<object>().ToList();
            var itemType = result[0].GetType();
            Assert.AreEqual(8, result.Count);
            Assert.AreEqual(1, result[0].GetValue<int>("e2.EmployeeId"));
            Assert.AreEqual("Adams", result[0].GetValue<string>("e1.last_name"));
            Assert.AreEqual(2, result[1].GetValue<int>("e2.EmployeeId"));
            Assert.AreEqual("Edwards", result[1].GetValue<string>("e1.last_name"));
        }
    }

    static public class ObjectExtensions
    {
        static public T GetValue<T>(this object obj, string fieldName)
        {
            return (T)Convert.ChangeType(
                obj.GetType().GetField(fieldName).GetValue(obj),
                typeof(T));
        }
    }
}
