using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Threading;
using Traficante.Connect.Connectors;

namespace Traficante.Connect.Tests.Connectors
{
    [TestClass]
    public class SqliteConnectorTests
    {
        private SqliteConnectorConfig config = new SqliteConnectorConfig
        {
            Alias = "chinook",
            Database = "chinook.db"
        };

       

        [TestMethod]
        public void TryConnect_RanWithoutException()
        {
            SqliteConnector connector = new SqliteConnector(this.config);
            connector.TryConnect(CancellationToken.None).Wait();
        }


        [TestMethod]
        public void GetTables()
        {
            SqliteConnector connector = new SqliteConnector(this.config);
            var tables = connector.GetTables().Result;
            Assert.IsTrue(tables.Any(x => x == "albums"));
        }

        [TestMethod]
        public void GetTableFields()
        {
            SqliteConnector connector = new SqliteConnector(this.config);
            var tables = connector.GetFields("Albums").Result;
            Assert.IsTrue(tables.Any(x => x.Name == "Title"));
        }


        [TestMethod]
        public void GetViews()
        {
            SqliteConnector connector = new SqliteConnector(this.config);
            var tables = connector.GetViews().Result;
            Assert.IsTrue(tables.Any(x => x == "vAlbums"));
        }


        [TestMethod]
        public void GetViewFields()
        {
            SqliteConnector connector = new SqliteConnector(this.config);
            var tables = connector.GetFields("vAlbums").Result;
            Assert.IsTrue(tables.Any(x => x.Name == "Title"));
        }
    }
}
