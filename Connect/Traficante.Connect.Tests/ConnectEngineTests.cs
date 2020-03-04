using Microsoft.VisualStudio.TestTools.UnitTesting;
using Traficante.Connect.Connectors;

namespace Traficante.Connect.Tests
{
    [TestClass]
    public class ConnectEngineTests
    {
        [TestMethod]
        public void SelectFrom_Csv_FromFile()
        {
            ConnectEngine connectEngine = new ConnectEngine();
            connectEngine.AddConector(new CsvConnectorConfig { Alias = "csv" });
            var table = connectEngine.Run("SELECT * FROM csv.fromFile('test.csv')");
            Assert.AreEqual(2, table.Count);
            Assert.AreEqual("1", table[0][0]);
            Assert.AreEqual("one", table[0][1]);
            Assert.AreEqual("2", table[1][0]);
            Assert.AreEqual("two", table[1][1]);
        }
    }
}
