using Microsoft.VisualStudio.TestTools.UnitTesting;
using Traficante.Connect.Connectors;
using System.Linq;

namespace Traficante.Connect.Tests.Connectors
{
    [TestClass]
    public class ExcelConnectorTests
    {
        [TestMethod]
        public void SelectAll()
        {
            var excelConnector = new ExcelConnector(new ExcelConnectorConfig { Alias = "exel" });
            var connectEngine = new ConnectEngine();
            connectEngine.AddConector(excelConnector);
            var results = connectEngine.Run("select * from exel.fromFile('employees.xlsx')").Cast<object>().ToList();
            Assert.AreEqual(8, results.Count);
        }

        [TestMethod]
        public void SelectAllFromSheet()
        {
            var excelConnector = new ExcelConnector(new ExcelConnectorConfig { Alias = "exel" });
            var connectEngine = new ConnectEngine();
            connectEngine.AddConector(excelConnector);
            var results = connectEngine.Run("select * from exel.fromFile('employees.xlsx','Sheet1')").Cast<object>().ToList();
            Assert.AreEqual(8, results.Count);
        }

        [TestMethod]
        public void GetSheets()
        {
            var excelConnector = new ExcelConnector(new ExcelConnectorConfig { Alias = "exel", FilePath = "employees.xlsx" });
            var sheets = excelConnector.GetSheets().Result.ToList();
            Assert.AreEqual("Sheet1", sheets[0]);
            Assert.AreEqual("Sheet2", sheets[1]);
            Assert.AreEqual("Sheet3", sheets[2]);
            Assert.AreEqual(3, sheets.Count);
        }
    }
}
