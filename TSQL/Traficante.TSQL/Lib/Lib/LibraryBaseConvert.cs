using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Text.RegularExpressions;
using Traficante.TSQL.Evaluator.Helpers;
using Traficante.TSQL.Lib.Attributes;

namespace Traficante.TSQL.Lib
{
    public partial class Library
    {
        private Dictionary<int, string> _parseDataTimeStyle = new Dictionary<int, string>
        {
            { 0, "MMMM d yyyy h:mtt" },
            { 100, "MMMM d yyyy h:mtt" },
            { 1, "M/d/yy" },
            { 101, "M/d/yyyy" },
            { 2, "yy.M.d" },
            { 102, "yyyy.M.d" },
            { 3, "d/M/yy" },
            { 103, "d/M/yyyy" },
            { 4, "d.M.yy" },
            { 104, "d.M.yyyy" },
            { 5, "d-M-yy" },
            { 105, "d-M-yyyy" },
            { 6, "d MMMM yy" },
            { 106, "d MMMM yyyy" },
            { 7, "MMMM d, yy" },
            { 107, "MMMM d, yyyy" },
            { 8, "H:m:s" },
            { 24, "H:m:s" },
            { 108, "H:m:s" },
            { 9, "MMMM d yyyy h:m:s:ffftt" },
            { 109, "MMMM d yyyy h:m:s:ffftt" },
            { 10, "M-d-yy" },
            { 110, "M-d-yyyy" },
            { 11, "yy/M/d" },
            { 111, "yyyy/M/d" },
            { 12, "yyMMdd" },
            { 112, "yyyyMMdd" },
            { 13, "d MMMM yyyy H:m:s:fff" },
            { 113, "d MMMM yyyy H:m:s:fff" },
            { 14, "H:m:s:fff" },
            { 114, "H:m:s:fff" },
            { 20, "yyyy-M-d H:m:s" },
            { 120, "yyyy-M-d H:m:s" },
            { 21, "yyyy-M-d H:m:s.fff" },
            { 25, "yyyy-M-d H:m:s.fff" },
            { 22, "M/d/yy h:m:s tt" },
            { 23, "yyyy-M-d" },
            { 121, "yyyy-M-d H:m:s.fff" },
            { 126, "yyyy-M-dTH:m:s.fff" },
            { 127, "yyyy-M-dTH:m:s.fff" },
            //{ 130, "d MMMM yyyy h:m:s:ffftt" },
            //{ 131, "dd/M/yyyy h:m:s:ffftt" }
        };

        private Dictionary<int, string> _toStringDataTimeStyle = new Dictionary<int, string>
        {
            { 0, "MMMM  d yyyy  h:mmtt" },
            { 100, "MMMM  d yyyy  h:mmtt" },
            { 1, "MM/dd/yy" },
            { 101, "MM/dd/yyyy" },
            { 2, "yy.MM.dd" },
            { 102, "yyyy.MM.dd" },
            { 3, "dd/MM/yy" },
            { 103, "dd/MM/yyyy" },
            { 4, "dd.MM.yy" },
            { 104, "dd.MM.yyyy" },
            { 5, "dd-MM-yy" },
            { 105, "dd-MM-yyyy" },
            { 6, "dd MMMM yy" },
            { 106, "dd MMMM yyyy" },
            { 7, "MMMM dd, yy" },
            { 107, "MMMM dd, yyyy" },
            { 8, "H:mm:ss" },
            { 24, "H:mm:ss" },
            { 108, "H:mm:ss" },
            { 9, "MMMM  d yyyy  h:mm:ss:ffftt" },
            { 109, "MMMM  d yyyy  h:mm:ss:ffftt" },
            { 10, "MM-dd-yy" },
            { 110, "MM-dd-yyyy" },
            { 11, "yy/MM/dd" },
            { 111, "yyyy/MM/dd" },
            { 12, "yyMMdd" },
            { 112, "yyyyMMdd" },
            { 13, "dd MMMM yyyy H:mm:ss:fff" },
            { 113, "dd MMMM yyyy H:mm:ss:fff" },
            { 14, "H:mm:ss:fff" },
            { 114, "H:mm:ss:fff" },
            { 20, "yyyy-MM-dd H:mm:ss" },
            { 120, "yyyy-MM-dd H:mm:ss" },
            { 21, "yyyy-MM-dd H:mm:ss.fff" },
            { 25, "yyyy-MM-dd H:mm:ss.fff" },
            { 22, "MM/dd/yy  h:mm:ss tt" },
            { 23, "yyyy-MM-dd" },
            { 121, "yyyy-MM-dd H:mm:ss.fff" },
            { 126, "yyyy-MM-ddTH:mm:ss.fff" },
            { 127, "yyyy-MM-ddTH:mm:ss.fff" },
            //{ 130, "d MMMM yyyy h:m:s:ffftt" },
            //{ 131, "dd/M/yyyy h:m:s:ffftt" }
        };

        [BindableMethod]
        public object Convert(Type type, object value)
        {
            return Convert(type, value, null);
        }

        [BindableMethod]
        public object Convert(Type type, object value, int? style)
        {
            if (value == null)
                return type.Default();

            var valueType = value?.GetType();
            var valueTypeCode = Type.GetTypeCode(valueType);

            if (type.IsAssignableFrom(valueType))
                return value;

            if (type == typeof(DateTimeOffset?))
            {
                if (value is DateTimeOffset?)
                    return value;
                if (value is DateTime?)
                    return new DateTimeOffset((DateTime)value);
                if (style.HasValue)
                {
                    if (_parseDataTimeStyle.TryGetValue(style.Value, out string dateFormat))
                    {
                        var str = Regex.Replace(value?.ToString(), @"\s+", " ");
                        if (string.IsNullOrEmpty(str))
                            return type.Default();
                        if (DateTimeOffset.TryParseExact(str, dateFormat, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out DateTimeOffset result))
                            return result;
                        else
                            throw new TSQLException($"Cannot convert '{str}' to datetime");
                    }
                    else
                        throw new TSQLException($"{style} is not a valid style when converting string to datetime");
                }
                else
                {
                    var str = Regex.Replace(value?.ToString(), @"\s+", " ");
                    if (string.IsNullOrEmpty(str))
                        return type.Default();
                    if (DateTimeOffset.TryParse(str, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out DateTimeOffset result))
                        return result;
                    else
                        throw new TSQLException($"Cannot convert '{str}' to datetime");
                }
            }

            if (type == typeof(DateTime?))
            {
                if (value is DateTime?)
                    return value;
                if (value is DateTimeOffset?)
                    return ((DateTimeOffset)value).DateTime;
                if (style.HasValue)
                {
                    if (_parseDataTimeStyle.TryGetValue(style.Value, out string dateFormat))
                    {
                        var str = Regex.Replace(value?.ToString(), @"\s+", " ");
                        if (string.IsNullOrEmpty(str))
                            return type.Default();
                        if (DateTime.TryParseExact(str, dateFormat, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out DateTime result))
                            return (result);
                        else
                            throw new TSQLException($"Cannot convert '{str}' to datetime");
                    }
                    else
                        throw new TSQLException($"{style} is not a valid style when converting string to datetime");
                }
                else
                {
                    var str = Regex.Replace(value?.ToString(), @"\s+", " ");
                    if (string.IsNullOrEmpty(str))
                        return type.Default();
                    if (DateTime.TryParse(str, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out DateTime result))
                        return (result);
                    else
                        throw new TSQLException($"Cannot convert '{str}' to datetime");
                }
            }

            if (type == typeof(string))
            {
                if (valueType == typeof(DateTimeOffset?) || valueType == typeof(DateTimeOffset))
                {
                    if (_toStringDataTimeStyle.TryGetValue(style.GetValueOrDefault(0), out string dateFormat))
                    {
                        DateTimeOffset dt = (DateTimeOffset)value;
                        return dt.ToString(dateFormat);
                    }
                    else
                        throw new TSQLException($"{style} is not a valid style when converting datetime to string");
                }
                if (valueType == typeof(DateTime?) || valueType == typeof(DateTime))
                {
                    if (_toStringDataTimeStyle.TryGetValue(style.GetValueOrDefault(0), out string dateFormat))
                    {
                        DateTime dt = (DateTime)value;
                        return dt.ToString(dateFormat);
                    }
                    else
                        throw new TSQLException($"{style} is not a valid style when converting datetime to string");
                }
                return value?.ToString();
            }

            if (type == typeof(bool?))
            {
                switch (valueTypeCode)
                {
                    case TypeCode.Byte:
                        return (((Byte)value) > 0);
                    case TypeCode.SByte:
                        return (((SByte)value) > 0);
                    case TypeCode.UInt16:
                        return (((UInt16)value) > 0);
                    case TypeCode.UInt32:
                        return (((UInt32)value) > 0);
                    case TypeCode.UInt64:
                        return (((UInt64)value) > 0);
                    case TypeCode.Int16:
                        return (((Int16)value) > 0);
                    case TypeCode.Int32:
                        return (((Int32)value) > 0);
                    case TypeCode.Int64:
                        return (((Int64)value) > 0);
                    case TypeCode.Decimal:
                        return (((Decimal)value) > 0);
                    case TypeCode.Double:
                        return (((Double)value) > 0);
                    case TypeCode.Single:
                        return (((Single)value) > 0);
                    case TypeCode.String:
                        var valueString = value.ToString();
                        if (Boolean.TryParse(valueString, out bool b))
                            return b;
                        if (Decimal.TryParse(valueString, out decimal d))
                            return (d > 0);
                        if (string.IsNullOrWhiteSpace(valueString))
                            return (false);
                        throw new TSQLException($"Cannot convert '{value.ToString()}' to data type bit.");
                }
                return true;   
            }

            var converter = TypeDescriptor.GetConverter(type);
            if (converter.CanConvertFrom(valueType))
                return converter.ConvertFrom(value);
            return converter.ConvertFromString(value?.ToString());
            throw new TSQLException($"Convert from {valueType?.Name} to {type.Name} is not implemented");

        }

        [BindableMethod]
        public object Cast(object value, Type type)
        {
            return Convert(type, value);
        }
    }
}
