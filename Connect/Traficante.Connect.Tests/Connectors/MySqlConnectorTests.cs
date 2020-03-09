using Microsoft.VisualStudio.TestTools.UnitTesting;
using Traficante.Connect.Connectors;

namespace Traficante.Connect.Tests.Connectors
{
    [TestClass]
    public class MySqlConnectorTests
    {
        //private MySqlConnectorConfig config = new MySqlConnectorConfig
        //{
        //    Alias = "mysql",
        //    UserId = "bcb1af4f67f723",
        //    Password = "4cea6888",
        //    Server = "eu-cdbr-west-02.cleardb.net"
        //    Database = "heroku_62f83aecaf54b07"
        //};

        private MySqlConnectorConfig config = new MySqlConnectorConfig
        {
            Alias = "mysql",
            UserId = "lmbwf4cy2b9sr25e",
            Password = "bnhrfyde4mm465pb",
            Server = "dz8959rne9lumkkw.chr7pe7iynqr.eu-west-1.rds.amazonaws.com:3306"
            //Database = "maidc1q2wm11dbzj"
        };

       

        [TestMethod]
        public void TryConnect_RanWithoutException()
        {
            MySqlConnector connector = new MySqlConnector(this.config);
            connector.TryConnect("maidc1q2wm11dbzj");
        }

        [TestMethod]
        public void GetDatabases()
        {
            MySqlConnector connector = new MySqlConnector(this.config);
            var databases = connector.GetDatabases();
            CollectionAssert.Contains(databases, "sys");
        }

        [TestMethod]
        public void GetTables()
        {
            MySqlConnector connector = new MySqlConnector(this.config);
            var tables = connector.GetTables("sys");
            CollectionAssert.Contains(tables, "sys_config");
        }


        [TestMethod]
        public void GetViews()
        {
            MySqlConnector connector = new MySqlConnector(this.config);
            var views = connector.GetViews("sys");
            CollectionAssert.Contains(views, "host_summary");
        }

    }
}
