using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;

namespace Traficante.Studio.Models
{
    public class AppDataSerializer
    {
        public string SerializeObjects(List<ObjectModel> objects)
        {
            return JsonConvert.SerializeObject(
                objects,
                Formatting.Indented,
                new StringEnumConverter(),
                new ObjectModelConverter());
        }

        public List<ObjectModel> DerializeObjects(string json)
        {
            return JsonConvert.DeserializeObject<List<ObjectModel>>(
                json,
                new StringEnumConverter(),
                new ObjectModelConverter());
        }
    }

    public class ObjectModelConverter : JsonConverter
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
            return typeof(ObjectModel).IsAssignableFrom(objectType);
        }
    }

}
