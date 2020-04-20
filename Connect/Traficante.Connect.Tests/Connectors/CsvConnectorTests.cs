using Microsoft.VisualStudio.TestTools.UnitTesting;
using Traficante.Connect.Connectors;
using System.Linq;

namespace Traficante.Connect.Tests.Connectors
{
    [TestClass]
    public class CsvConnectorTests
    {
        [TestMethod]
        public void FromCsv()
        {
            var csvConnector = new CsvConnector(new CsvConnectorConfig { Alias = "csv" });
            var connectEngine = new ConnectEngine();
            connectEngine.AddConector(csvConnector);
            var results = connectEngine.Run("select * from csv.fromFile('employees.csv')").Cast<object>().ToList();
            Assert.IsTrue(results.Count == 8);
        }
    }
}
