using Microsoft.VisualStudio.TestTools.UnitTesting;
using Traficante.Connect.Connectors;
using System.Linq;
using System.Collections.Generic;

namespace Traficante.Connect.Tests.Connectors
{
    [TestClass]
    public class CsvHelperTests
    {
        [TestMethod]
        public void FromCsv()
        {
            var csvConnector = new FilesConnector(new FilesConnectorConfig
            {
                Alias = "csv",
                Files = new List<FileConnectorConfig> {
                    new FileConnectorConfig { Name = "employees.csv", Path = "employees.csv" }
                }
            });
            var connectEngine = new ConnectEngine();
            connectEngine.AddConector(csvConnector);
            var results = connectEngine.Run("select * from csv.[employees.csv]").Cast<object>().ToList();
            Assert.AreEqual(8, results.Count);
        }
    }
}
