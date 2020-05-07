using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Traficante.Connect.Connectors;

namespace Traficante.Connect.Tests.Connectors
{
    [TestClass]
    public class ElasticSearchConnectorTests
    {
        private ElasticSearchConnectorConfig config = new ElasticSearchConnectorConfig
        {
            Alias = "elastic",
            Server = "http://tstldnvmgsaweb1:9200/"
        };


        [TestMethod]
        public void GetIndices()
        {
            var sut = new ElasticSearchConnector(config);
            var indices = sut.GetIndices().Result;
            Assert.IsTrue(indices.ToList().Count > 0);
        }

        [TestMethod]
        public void GetAliases()
        {
            var sut = new ElasticSearchConnector(config);
            var indices = sut.GetAliases().Result;
            Assert.IsTrue(indices.ToList().Count > 0);
        }

        [TestMethod]
        public void SelectAllFromElastic()
        {
            ConnectEngine engine = new ConnectEngine();
            engine.AddConector(this.config);

            var results = engine.Run("SELECT * FROM [elastic].[]");
        }

        [TestMethod]
        public void ElasticSearchDataReader()
        {
            using (var sut = new ElasticSearchDataReader(
                new ElasticSearchConnector(config),
                "",
                null, 
                "{ \"query\": { \"match_all\": {} } }",
                CancellationToken.None))
            {
                List<(string Name,Type Type)> fields = Enumerable
                    .Range(0, sut.FieldCount)
                    .Select(i => (sut.GetName(i), sut.GetFieldType(i)))
                    .ToList();

                if (sut.Read())
                {
                    for(int i = 0; i < sut.FieldCount; i++)
                    {
                        var field = fields[i];
                        var value = sut.GetValue(i);
                        if (value != null)
                        {
                            Assert.AreEqual(field.Type, value.GetType());
                        }
                    }
                    
                }
            }

            
        }
    }
}
