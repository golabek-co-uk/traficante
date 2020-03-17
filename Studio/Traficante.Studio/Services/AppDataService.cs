using DynamicData;
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
            var appData = LoadFromFile();
            appData
                .Objects
                .ToObservableChangeSet()
                .OnItemAdded(x =>
                {
                    x.PropertyChanged += (x, y) =>
                    {
                        SaveToFile(appData);
                    };
                })
                .Subscribe(x =>
                {
                    SaveToFile(appData);
                });

            appData
                .Queries
                .ToObservableChangeSet()
                .OnItemAdded(x =>
                {
                    x.PropertyChanged += (x, y) =>
                    {
                        SaveToFile(appData);
                    };
                })
                .Subscribe(x =>
                {
                    SaveToFile(appData);
                });
            

            return appData;
        }

        public void SaveToFile(AppData appData)
        {
            try
            {
                var json = new AppDataSerializer().Serialize(appData);
                File.WriteAllText("AppData", json);
            } catch { }
        }

        public AppData LoadFromFile()
        {
            try
            {
                var json = File.ReadAllText("AppData");
                return new AppDataSerializer().Derialize<AppData>(json);
            }
            catch { }
            return new AppData();
        }
    }

    public class AppDataSerializer
    {
        public string Serialize<T>(T appData)
        {
            return JsonConvert.SerializeObject(
                appData,
                Formatting.Indented,
                new StringEnumConverter(),
                new TypeConverter<ObjectModel>());
        }

        public T Derialize<T>(string json)
        {
            return JsonConvert.DeserializeObject<T>(
                json,
                new StringEnumConverter(),
                new TypeConverter<ObjectModel>());
        }

        public T Clone<T>(T appData)
        {
            var json = JsonConvert.SerializeObject(
                appData,
                Formatting.Indented,
                new StringEnumConverter(),
                new TypeConverter<ObjectModel>());

            return JsonConvert.DeserializeObject<T>(
                json,
                new StringEnumConverter(),
                new TypeConverter<ObjectModel>());
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
