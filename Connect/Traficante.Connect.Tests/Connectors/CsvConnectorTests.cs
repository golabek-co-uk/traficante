using Microsoft.VisualStudio.TestTools.UnitTesting;
using Traficante.Connect.Connectors;

namespace Traficante.Connect.Tests.Connectors
{
    [TestClass]
    public class CsvConnectorTests
    {
        [TestMethod]
        public void Test()
        {
            var connector = new CsvConnector(new CsvConnectorConfig());
            //var items = connector.RunSelect("select * from csv('Services/data.csv')");
        }
    }
}
