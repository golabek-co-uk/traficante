using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Traficante.Connect.Connectors
{
    public class ElasticSearchConnector : Connector
    {
        public ElasticSearchConnectorConfig Config => (ElasticSearchConnectorConfig)base.Config;

        public ElasticSearchConnector(ElasticSearchConnectorConfig config)
        {
            base.Config = config;
        }

        public async Task TryConnectAsync(CancellationToken ct)
        {
            using (HttpClient httpClient = new HttpClient())
            {
                var result = await httpClient.GetAsync(ToFullUrl("/"));
                result.EnsureSuccessStatusCode();
            }
        }

        public void TryConnect()
        {
            using (HttpClient httpClient = new HttpClient())
            {
                var result = httpClient.GetAsync(ToFullUrl("/")).Result;
                result.EnsureSuccessStatusCode();
            }
        }

        public IEnumerable<string> GetIndices()
        {
            using (HttpClient httpClient = new HttpClient())
            {
                var result = httpClient.GetAsync(ToFullUrl("/_cat/indices?format=json")).Result;
                result.EnsureSuccessStatusCode();
                var jsonString = result.Content.ReadAsStringAsync().Result;
                using (var jsonDoc = JsonDocument.Parse(jsonString))
                {
                    return jsonDoc
                        .RootElement
                        .EnumerateArray()
                        .Select(x => x.GetProperty("index").GetString())
                        .OrderBy(x => x)
                        .ToList();
                }
            }
        }

        public IEnumerable<string> GetAliases()
        {
            using (HttpClient httpClient = new HttpClient())
            {
                var result = httpClient.GetAsync(ToFullUrl("/_cat/aliases?format=json")).Result;
                result.EnsureSuccessStatusCode();
                var jsonString = result.Content.ReadAsStringAsync().Result;
                using (var jsonDoc = JsonDocument.Parse(jsonString))
                {
                    return jsonDoc
                        .RootElement
                        .EnumerateArray()
                        .Select(x => x.GetProperty("alias").GetString())
                        .OrderBy(x => x)
                        .ToList();
                }
            }
        }

        public JsonDocument GetIndexMapping(string index)
        {
            using (HttpClient httpClient = new HttpClient())
            {
                var result = httpClient.GetAsync(ToFullUrl(index)).Result;
                result.EnsureSuccessStatusCode();
                var jsonString = result.Content.ReadAsStringAsync().Result;
                return JsonDocument.Parse(jsonString);
            }
        }

        public string ToFullUrl(string path)
        {
            var url = new Uri(new Uri(this.Config.Server), path);
            return url.ToString();
        }
    }

    public class ElasticSearchConnectorConfig : ConnectorConfig
    {
        public string Server { get; set; }
    }
}
