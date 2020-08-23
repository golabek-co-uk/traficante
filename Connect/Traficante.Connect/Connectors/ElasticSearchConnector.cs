using System;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Traficante.TSQL;

namespace Traficante.Connect.Connectors
{
    public class ElasticSearchConnector : Connector
    {
        new public ElasticSearchConnectorConfig Config => (ElasticSearchConnectorConfig)base.Config;

        public ElasticSearchConnector(ElasticSearchConnectorConfig config)
        {
            base.Config = config;
        }

        public async Task TryConnect(CancellationToken ct = default)
        {
            using (HttpClient httpClient = new HttpClient())
            {
                var result = await httpClient.GetAsync(ToFullUrl("/"), ct);
                result.EnsureSuccessStatusCode();
            }
        }

        public override Delegate ResolveTable(string[] path, CancellationToken ct)
        {
            if (path.Length > 3)
                throw new Exception($"Incorrect Elasticsearch path {string.Join(" -> ", path)}");

            string indexName = null;
            string typeName = null;
            if (path.Length == 3)
            {
                typeName = path[2];
                indexName = path[1];
            }
            if (path.Length == 2)
            {
                typeName = null;
                indexName = path[1];
            }

            Func <IDataReader> @delegate = () =>
            {
                return new ElasticSearchDataReader(this, indexName, typeName, "{ \"query\": { \"match_all\": {} } }", ct);
            };
            return @delegate;
        }

        public async Task<IEnumerable<string>> GetIndices()
        {
            using (HttpClient httpClient = new HttpClient())
            {
                var result = await httpClient.GetAsync(ToFullUrl("/_cat/indices?format=json"));
                result.EnsureSuccessStatusCode();
                var jsonString = await result.Content.ReadAsStringAsync();
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

        public async Task<IEnumerable<string>> GetAliases()
        {
            using (HttpClient httpClient = new HttpClient())
            {
                var result = await httpClient.GetAsync(ToFullUrl("/_cat/aliases?format=json"));
                result.EnsureSuccessStatusCode();
                var jsonString = await result.Content.ReadAsStringAsync();
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

        public async Task<JsonDocument> GetIndex(string index)
        {
            using (HttpClient httpClient = new HttpClient())
            {
                var result = await httpClient.GetAsync(ToFullUrl(index));
                result.EnsureSuccessStatusCode();
                var jsonString = await result.Content.ReadAsStringAsync();
                return JsonDocument.Parse(jsonString);
            }
        }

        public async Task<IEnumerable<(string Name, string MappingType, string FieldType)>> GetFields(string index)
        {
            using (var jsonIndex = await GetIndex(index))
            {
                return GetFields(jsonIndex);
            }
        }

        public IEnumerable<(string Name, string MappingType, string FieldType)> GetFields(JsonDocument index)
        {
            var mappings = index.RootElement.EnumerateObject().First().Value.GetProperty("mappings");
            var types = mappings.EnumerateObject().ToList();
            return types
                .Select(t =>
                {
                    return t.Value
                        .GetProperty("properties")
                        .EnumerateObject()
                        .Select(p => (p.Name, t.Name, p.Value.GetProperty("type").GetString()))
                        .ToList();
                })
                .SelectMany(x => x)
                .ToList();
        }

        public IEnumerable<string> GetMappingTypes(JsonDocument index)
        {
            var mappings = index.RootElement.EnumerateObject().First().Value.GetProperty("mappings");
            var types = mappings.EnumerateObject().ToList();
            return types.Select(t => t.Name).ToList();
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

    public class ElasticSearchDataReader : IAsyncDataReader
    {
        private ElasticSearchConnector _elasticSearch;
        private string _indexName;
        private string _mappingTypeName;
        private string _query;
        private readonly CancellationToken _ct;
        private JsonDocument _index;
        private List<(string Name, string MappingType, string FieldType)> _fields;
        private string _scrollId = null;
        private Queue<JsonElement> _documents = new Queue<JsonElement>();
        private JsonElement? _currentDocument = null;


        public ElasticSearchDataReader(ElasticSearchConnector elasticSearch, string indexName, string mappingTypeName, string query, CancellationToken ct)
        {
            this._elasticSearch = elasticSearch;
            this._indexName = indexName;
            this._mappingTypeName = mappingTypeName;
            this._query = query;
            this._ct = ct;
            this._index = elasticSearch.GetIndex(indexName).Result;

            var mappingTypes = elasticSearch.GetMappingTypes(_index).ToList();
            if (mappingTypes.Count > 0)
                this._mappingTypeName = mappingTypes.First();

            if (this._mappingTypeName != null)
                this._fields = elasticSearch.GetFields(_index).Where(x => x.MappingType == _mappingTypeName).ToList();
        }

        public object this[int i] => throw new NotImplementedException();

        public object this[string name] => throw new NotImplementedException();

        public int Depth => throw new NotImplementedException();

        public bool IsClosed => throw new NotImplementedException();

        public int RecordsAffected => throw new NotImplementedException();

        public int FieldCount => _fields.Count;

        public void Close()
        {
        }

        public void Dispose()
        {
        }

        public bool GetBoolean(int i)
        {
            return (bool)this.GetValue(i);
        }

        public byte GetByte(int i)
        {
            return (byte)this.GetValue(i);
        }

        public char GetChar(int i)
        {
            return (char)this.GetValue(i);
        }

        public DateTime GetDateTime(int i)
        {
            return (DateTime)this.GetValue(i);
        }

        public decimal GetDecimal(int i)
        {
            return (decimal)this.GetValue(i);
        }

        public double GetDouble(int i)
        {
            return (double)this.GetValue(i);
        }

        public float GetFloat(int i)
        {
            return (float)this.GetValue(i);
        }

        public Guid GetGuid(int i)
        {
            return (Guid)this.GetValue(i);
        }

        public short GetInt16(int i)
        {
            return (short)this.GetValue(i);
        }

        public int GetInt32(int i)
        {
            return (int)this.GetValue(i);
        }

        public long GetInt64(int i)
        {
            return (long)this.GetValue(i);
        }

        public long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length)
        {
            throw new NotImplementedException();
        }

        public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
        {
            throw new NotImplementedException();
        }

        public string GetString(int i)
        {
            return (string)this.GetValue(i);
        }

        public object GetValue(int i)
        {
            var fieldName = this._fields[i].Name;
            JsonElement value;
            if (_currentDocument.Value.GetProperty("_source").TryGetProperty(fieldName, out value))
            {
                //if (value.ValueKind == JsonValueKind.String)
                //    return value.ToString();
                //if (value.ValueKind == JsonValueKind.False)
                //    return false;
                //if (value.ValueKind == JsonValueKind.True)
                //    return true;
                //if (value.ValueKind == JsonValueKind.Number)
                //    return long.Parse(value.ToString());
                //if (value.ValueKind == JsonValueKind.Undefined)
                //    return null;
                //if (value.ValueKind == JsonValueKind.Null)
                //    return null;
                return value;
            }
            return null;
        }

        public int GetValues(object[] values)
        {
            int count = Math.Min(values.Length, _fields.Count);
            for (int i = 0; i < count; i++)
            {
                values[i] = GetValue(i);
            }
            return count;
        }

        public IDataReader GetData(int i)
        {
            return null;
        }

        public Type GetFieldType(int i)
        {
            //var fieldName = this._fields[i].FieldType;
            //if (fieldName == "text" || 
            //    fieldName == "keyword")
            //    return typeof(string);
            //if (fieldName == "long" || 
            //    fieldName == "integer" || 
            //    fieldName == "short" || 
            //    fieldName == "byte" ||
            //    fieldName == "double" ||
            //    fieldName == "float" ||
            //    fieldName == "half_float" ||
            //    fieldName == "scaled_float")
            //    return typeof(long);
            //if (fieldName == "date" || fieldName == "date_nanos")
            //    return typeof(DateTime);
            //if (fieldName == "boolean" || fieldName == "binary")
            //    return typeof(bool);
            //if (fieldName == "Object")
            //    return typeof(JsonElement);
            //if (fieldName == "Nested")
            //    return typeof(JsonElement[]);
            return typeof(JsonElement?);
        }

        public string GetDataTypeName(int i)
        {
            return GetFieldType(i).Name;
        }

        public string GetName(int i)
        {
            return _fields[i].Name;
        }

        public int GetOrdinal(string name)
        {
            return _fields.FindIndex(x => x.Name == name);
        }

        public System.Data.DataTable GetSchemaTable()
        {
            return null;
        }


        public bool IsDBNull(int i)
        {
            throw new NotImplementedException();
        }

        public bool NextResult()
        {
            return NextResultAsync().Result;
        }

        public bool Read()
        {
            if (_documents.Count > 0)
            {
                _currentDocument = _documents.Dequeue();
                return true;
            }

            _currentDocument = null;
            return false;
        }

        public async Task<bool> ReadAsync()
        {
            return await Task.FromResult(Read());
        }

        public async Task<bool> NextResultAsync()
        {
            if (_scrollId == null)
            {
                using (HttpClient client = new HttpClient())
                {
                    var response = await client.PostAsync(
                        this._elasticSearch.ToFullUrl($"/{this._indexName}/_search?scroll=10m"),
                        new StringContent(this._query, Encoding.UTF8, "application/json"),
                        this._ct);
                    response.EnsureSuccessStatusCode();
                    var jsonString = await response.Content.ReadAsStringAsync();
                    var results = ParseJsonResults(jsonString);
                    this._scrollId = results.ScrollId;
                    results.Hits.ForEach(x => this._documents.Enqueue(x));
                    return this._scrollId != null && results.Hits.Count > 0;
                }
            }
            else
            {
                using (HttpClient client = new HttpClient())
                {
                    var response = await client.PostAsync(
                            this._elasticSearch.ToFullUrl("/_search/scroll"),
                            new StringContent("{\"scroll\" : \"1m\", \"scroll_id\" : \"" + _scrollId + "\"}", Encoding.UTF8, "application/json"),
                            this._ct);
                    response.EnsureSuccessStatusCode();
                    var jsonString = await response.Content.ReadAsStringAsync();
                    var results = ParseJsonResults(jsonString);
                    this._scrollId = results.ScrollId;
                    results.Hits.ForEach(x => this._documents.Enqueue(x));
                    return this._scrollId != null && results.Hits.Count > 0;
                }
            }
        }

        public (string ScrollId, List<JsonElement> Hits) ParseJsonResults(string jsonResults)
        {
            var jsonDocument = JsonDocument.Parse(jsonResults);
            var scrollId = jsonDocument.RootElement.GetProperty("_scroll_id").ToString();
            var hits = jsonDocument.RootElement.GetProperty("hits").GetProperty("hits").EnumerateArray().ToList();
            return (scrollId, hits);
        }
    }

}
