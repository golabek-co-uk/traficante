using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using Traficante.Connect.Connectors;

namespace Traficante.Connect.Tests.Connectors
{
    // MariaDB Sakila test database
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
            UserId = "root",
            Password = "qwerty",
            Server = "UATLDNVMES1"
            //Database = "maidc1q2wm11dbzj"
        };

       

        [TestMethod]
        public void TryConnect_Server()
        {
            MySqlConnector connector = new MySqlConnector(this.config);
            connector.TryConnect("sakila").Wait();
        }

        [TestMethod]
        public void GetDatabases()
        {
            MySqlConnector connector = new MySqlConnector(this.config);
            var databases = connector.GetDatabases().Result.ToList();
            CollectionAssert.Contains(databases, "sakila");
        }

        [TestMethod]
        public void GetTables()
        {
            MySqlConnector connector = new MySqlConnector(this.config);
            var tables = connector.GetTables("sakila").Result.ToList();
            CollectionAssert.Contains(tables, "actor");
        }


        [TestMethod]
        public void GetViews()
        {
            MySqlConnector connector = new MySqlConnector(this.config);
            var views = connector.GetViews("sakila").Result.ToList();
            CollectionAssert.Contains(views, "actor_info");
        }

    }
}
