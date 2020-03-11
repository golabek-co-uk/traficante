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
            connector.TryConnectAsync(CancellationToken.None).Wait();
        }

        [TestMethod]
        public void GetDatabases()
        {
            SqliteConnector connector = new SqliteConnector(this.config);
            var databases = connector.GetDatabases();
            CollectionAssert.Contains(databases, "chinook.db");
        }

        [TestMethod]
        public void GetTables()
        {
            SqliteConnector connector = new SqliteConnector(this.config);
            var tables = connector.GetTables();
            Assert.IsTrue(tables.Any(x => x.name == "albums"));
        }


        [TestMethod]
        public void GetViews()
        {
            SqliteConnector connector = new SqliteConnector(this.config);
            var tables = connector.GetViews();
            Assert.IsTrue(tables.Any(x => x.name == "vAlbums"));
        }

    }
}
