using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Traficante.Connect.Connectors;

namespace Traficante.Connect.Tests.Connectors
{
    [TestClass]
    public class ElasticSearchConnectorTests
    {
        private ElasticSearchConnectorConfig config = new ElasticSearchConnectorConfig
        {
            Server = "http://tstldnvmgsaweb1:9200/"
            //Database = "maidc1q2wm11dbzj"
        };


        [TestMethod]
        public void GetIndices()
        {
            var sut = new ElasticSearchConnector(config);
            var indices = sut.GetIndices();
            Assert.IsTrue(indices.ToList().Count > 0);
        }

        [TestMethod]
        public void GetAliases()
        {
            var sut = new ElasticSearchConnector(config);
            var indices = sut.GetAliases();
            Assert.IsTrue(indices.ToList().Count > 0);
        }
    }
}
