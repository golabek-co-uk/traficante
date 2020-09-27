using Microsoft.VisualStudio.TestTools.UnitTesting;
using Traficante.Connect.Connectors;
using System.Linq;
using System.Collections.Generic;
using System.Collections;

namespace Traficante.Connect.Tests.Connectors
{
    [TestClass]
    public class ExcelHelperTests
    {
        [TestMethod]
        public void SelectAll()
        {
            var excelConnector = new FilesConnector(new FilesConnectorConfig 
            { 
                Alias = "excel",
                Files = new List<FileConnectorConfig> {
                    new FileConnectorConfig { Name = "employees.xlsx", Path = "employees.xlsx" }
                }
            });
            var connectEngine = new ConnectEngine();
            connectEngine.AddConector(excelConnector);
            var results = ((IEnumerable)connectEngine.Run("select * from excel.[employees.xlsx]")).Cast<object>().ToList();
            Assert.AreEqual(9, results.Count);
        }

        [TestMethod]
        public void SelectAllFromSheet()
        {
            var excelConnector = new FilesConnector(new FilesConnectorConfig
            {
                Alias = "excel",
                Files = new List<FileConnectorConfig> {
                    new FileConnectorConfig { Name = "employees.xlsx", Path = "employees.xlsx" }
                }
            });
            var connectEngine = new ConnectEngine();
            connectEngine.AddConector(excelConnector);
            var results = ((IEnumerable)connectEngine.Run("select * from excel.[employees.xlsx].Sheet1")).Cast<object>().ToList();
            Assert.AreEqual(9, results.Count);
        }

        [TestMethod]
        public void GetSheets()
        {
            var sheets = new ExcelHelper().GetSheets("employees.xlsx").Result.ToList();
            Assert.AreEqual("Sheet1", sheets[0]);
            Assert.AreEqual("Sheet2", sheets[1]);
            Assert.AreEqual("Sheet3", sheets[2]);
            Assert.AreEqual(3, sheets.Count);
        }
    }
}
