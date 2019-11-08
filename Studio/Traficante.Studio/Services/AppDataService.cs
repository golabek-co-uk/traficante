using DynamicData.Binding;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using Traficante.Studio.Models;

namespace Traficante.Studio.Services
{
    public class AppDataService
    {
        public AppData Load()
        {
            var appData = new AppData();
            appData.Objects = LoadObjects();
            appData
                .Objects
                .ToObservableChangeSet()
                .Subscribe(x =>
                {
                    SaveObjects(appData.Objects);
                });
            appData.Queries = LoadQueries();
            
            appData
                .Queries
                .ToObservableChangeSet()
                .Subscribe(x =>
                {
                    SaveQueries(appData.Queries);
                });
            return appData;
        }

        public ObservableCollection<QueryModel> LoadQueries()
        {
            try
            {
                var queriesJson = File.ReadAllText("Queries");
                var queries = new AppDataSerializer().DerializeQueries(queriesJson);
                queries.ForEach(x =>
                {
                    if (string.IsNullOrEmpty(x.Id))
                        x.Id = Guid.NewGuid().ToString();
                });
                return new ObservableCollection<QueryModel>(queries);
            }
            catch { }
            return new ObservableCollection<QueryModel>();
        }

        public void SaveQueries(ObservableCollection<QueryModel> queriesCollection)
        {
            try
            {
                var queries = queriesCollection.ToList();
                var queriesJson = new AppDataSerializer().SerializeQueries(queries);
                File.WriteAllText("Queries", queriesJson);
            }
            catch { }
        }

        public ObservableCollection<ObjectModel> LoadObjects()
        {
            try
            {
                var objectsJson = File.ReadAllText("Objects");
                var objects = new AppDataSerializer().DerializeObjects(objectsJson);
                return new ObservableCollection<ObjectModel>(objects);
            }
            catch { }
            return new ObservableCollection<ObjectModel>();
        }

        public void SaveObjects(ObservableCollection<ObjectModel> objectsCollection)
        {
            try
            {
                var objects = objectsCollection.ToList();
                var objectsJson = new AppDataSerializer().SerializeObjects(objects);
                File.WriteAllText("Objects", objectsJson);
            }
            catch { }
        }
    }

    public class AppDataSerializer
    {
        public string SerializeObjects(List<ObjectModel> objects)
        {
            return JsonConvert.SerializeObject(
                objects,
                Formatting.Indented,
                new StringEnumConverter(),
                new TypeConverter<ObjectModel>());
        }

        public List<ObjectModel> DerializeObjects(string json)
        {
            return JsonConvert.DeserializeObject<List<ObjectModel>>(
                json,
                new StringEnumConverter(),
                new TypeConverter<ObjectModel>());
        }

        public string SerializeQueries(List<QueryModel> queries)
        {
            return JsonConvert.SerializeObject(
                queries,
                Formatting.Indented,
                new StringEnumConverter(),
                new TypeConverter<QueryModel>());
        }

        public List<QueryModel> DerializeQueries(string json)
        {
            return JsonConvert.DeserializeObject<List<QueryModel>>(
                json,
                new StringEnumConverter(),
                new TypeConverter<QueryModel>());
        }
    }

    public class TypeConverter<T> : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("$type");
            writer.WriteValue(value.GetType().Name);
            writer.WritePropertyName("$value");
            JToken valueJToken = JToken.FromObject(value);
            valueJToken.WriteTo(writer);
            writer.WriteEndObject();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            reader.Read();
            var typeName = reader.ReadAsString();
            reader.Read();
            var type = Type.GetType("Traficante.Studio.Models." + typeName);
            reader.Read();
            var jToken = JToken.ReadFrom(reader);
            var value = jToken.ToObject(type);
            reader.Read();
            return value;
        }

        public override bool CanRead
        {
            get { return true; }
        }

        public override bool CanConvert(Type objectType)
        {
            return typeof(T).IsAssignableFrom(objectType);
        }
    }

}
