//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using System;
//using System.Collections.Generic;
//using System.Text;
//using Traficante.Studio.Models;
//using Traficante.Studio.Services;

//namespace Traficante.Studio.Tests.Services
//{
//    [TestClass]
//    public class MySqlServiceTests
//    {
//        MySqlConnectionInfo connectionInfo = new MySqlConnectionInfo
//        {
//            Server = "localhost",
//            UserId = "test",
//            Password = "test"
//        };

//        [TestMethod]
//        public void GetDatabases()
//        {
//            MySqlService mySqlService = new MySqlService();
//            var databases = mySqlService.GetDatabases(connectionInfo);
//            CollectionAssert.Contains(databases, "sys");
//        }

//        [TestMethod]
//        public void GetTables()
//        {
//            MySqlService mySqlService = new MySqlService();
//            var tables = mySqlService.GetTables(connectionInfo, "sys");
//            CollectionAssert.Contains(tables, "sys_config");
//        }


//        [TestMethod]
//        public void GetViews()
//        {
//            MySqlService mySqlService = new MySqlService();
//            var views = mySqlService.GetViews(connectionInfo, "sys");
//            CollectionAssert.Contains(views, "host_summary");
//        }
//    }
//}
