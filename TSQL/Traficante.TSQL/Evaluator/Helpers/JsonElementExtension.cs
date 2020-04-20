using System;
using System.Text.Json;

namespace Traficante.TSQL.Evaluator.Helpers
{
    static public class JsonElementExtension
    {
        static public JsonElement? GetPropertyOrDefault(this JsonElement? jsonElement, string property)
        {
            if (jsonElement.HasValue && jsonElement.Value.TryGetProperty(property, out JsonElement output))
                return output;
            return null;
        }

        static public Int32? GetValueInt32(this JsonElement? jsonElement)
        {
            try
            {
                if (jsonElement.HasValue)
                {
                    if (jsonElement.Value.TryGetInt32(out Int32 value))
                        return value;
                }
                return null;
            } catch
            {
                return jsonElement.ConvertTo<Int32?>();
            }
        }

        static public Int64? GetValueInt64(this JsonElement? jsonElement)
        {
            try
            {
                if (jsonElement.HasValue)
                {
                    if (jsonElement.Value.TryGetInt64(out Int64 value))
                        return value;
                }
                return null;
            }
            catch
            {
                return jsonElement.ConvertTo<Int64?>();
            }
        }

        static public Byte? GetValueByte(this JsonElement? jsonElement)
        {
            try {
            if (jsonElement.HasValue)
            {
                if (jsonElement.Value.TryGetByte(out Byte value))
                    return value;
            }
            return null;
            }
            catch
            {
                return jsonElement.ConvertTo<Byte?>();
            }
        }

        static public DateTime? GetValueDateTime(this JsonElement? jsonElement)
        {
            try
            {
                if (jsonElement.HasValue)
                {
                    if (jsonElement.Value.TryGetDateTime(out DateTime value))
                        return value;
                }
                return null;
            }
            catch
            {
                return jsonElement.ConvertTo<DateTime?>();
            }
        }

        static public DateTimeOffset? GetValueDateTimeOffset(this JsonElement? jsonElement)
        {
            try
            {
            if (jsonElement.HasValue)
            {
                if (jsonElement.Value.TryGetDateTimeOffset(out DateTimeOffset value))
                    return value;
            }
            return null;
            }
            catch
            {
                return jsonElement.ConvertTo<DateTimeOffset?>();
            }
        }

        static public Decimal? GetValueDecimal(this JsonElement? jsonElement)
        {
            try {
            if (jsonElement.HasValue)
            {
                if (jsonElement.Value.TryGetDecimal(out Decimal value))
                    return value;
            }
            return null;
            }
            catch
            {
                return jsonElement.ConvertTo<Decimal?>();
            }
        }

        static public Double? GetValueDouble(this JsonElement? jsonElement)
        {
            try
            {
            if (jsonElement.HasValue)
            {
                if (jsonElement.Value.TryGetDouble(out Double value))
                    return value;
            }
            return null;
            }
            catch
            {
                return jsonElement.ConvertTo<Double?>();
            }
        }

        static public Int16? GetValueInt16(this JsonElement? jsonElement)
        {
            try
            {
            if (jsonElement.HasValue)
            {
                if (jsonElement.Value.TryGetInt16(out Int16 value))
                    return value;
            }
            return null;
            }
            catch
            {
                return jsonElement.ConvertTo<Int16?>();
            }
        }

        static public SByte? GetValueSByte(this JsonElement? jsonElement)
        {
            try
            { 
            if (jsonElement.HasValue)
            {
                if (jsonElement.Value.TryGetSByte(out SByte value))
                    return value;
            }
            return null;
            }
            catch
            {
                return jsonElement.ConvertTo<SByte?>();
            }
        }

        static public Single? GetValueSingle(this JsonElement? jsonElement)
        {
            try
            {
            if (jsonElement.HasValue)
            {
                if (jsonElement.Value.TryGetSingle(out Single value))
                    return value;
            }
            return null;
            }
            catch
            {
                return jsonElement.ConvertTo<Single?>();
            }
        }

        static public UInt16? GetValueUInt16(this JsonElement? jsonElement)
        {
            try
            {
            if (jsonElement.HasValue)
            {
                if (jsonElement.Value.TryGetUInt16(out UInt16 value))
                    return value;
            }
            return null;
            }
            catch
            {
                return jsonElement.ConvertTo<UInt16?>();
            }
        }

        static public UInt32? GetValueUInt32(this JsonElement? jsonElement)
        {
            try
            {
            if (jsonElement.HasValue)
            {
                if (jsonElement.Value.TryGetUInt32(out UInt32 value))
                    return value;
            }
            return null;
            }
            catch
            {
                return jsonElement.ConvertTo<UInt32?>();
            }
        }

        static public string GetValueString(this JsonElement? jsonElement)
        {
            try
            {
            if (jsonElement.HasValue)
            {
                return jsonElement.Value.GetString();
            }
            return null;
            }
            catch
            {
                return jsonElement.ConvertTo<string>();
            }
        }

        static public UInt64? GetValueUInt64(this JsonElement? jsonElement)
        {
            try
            {
                if (jsonElement.HasValue)
                {
                    if (jsonElement.Value.TryGetUInt64(out UInt64 value))
                        return value;
                }
                return null;
            }
            catch
            {
                return jsonElement.ConvertTo<UInt64?>();
            }

        }
    }
}