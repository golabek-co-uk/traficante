using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Traficante.TSQL.Evaluator.Tables;
using Traficante.TSQL.Lib;
using Traficante.TSQL.Schema;
using Traficante.TSQL.Schema.DataSources;
using Traficante.TSQL.Schema.Reflection;

namespace Traficante.TSQL.Evaluator.Helpers
{
    public static class EvaluationHelper
    {
        public static IEnumerable<(string FieldName, Type Type)> CreateTypeComplexDescription(
            string initialFieldName, Type type)
        {
            var output = new List<(string FieldName, Type Type)>();
            var fields = new Queue<(string FieldName, Type Type, int Level)>();

            fields.Enqueue((initialFieldName, type, 0));
            output.Add((initialFieldName, type));

            while (fields.Count > 0)
            {
                var current = fields.Dequeue();

                if(current.Level > 3)
                    continue;

                foreach (var prop in current.Type.GetProperties())
                {
                    if (prop.MemberType != MemberTypes.Property)
                        continue;

                    var complexName = $"{current.FieldName}.{prop.Name}";
                    output.Add((complexName, prop.PropertyType));

                    if(prop.PropertyType == current.Type)
                        continue;

                    if (!(prop.PropertyType.IsPrimitive || prop.PropertyType == typeof(string) || prop.PropertyType == typeof(object)))
                    {
                        fields.Enqueue((complexName, prop.PropertyType, current.Level + 1));
                    }
                }
            }

            return output;
        }

        public static string GetCastableType(Type type)
        {
            if (type.IsGenericType) return GetFriendlyTypeName(type);

            return $"{type.Namespace}.{type.Name}";
        }

        public static Type[] GetNestedTypes(Type type)
        {
            if (!type.IsGenericType)
                return new[] {type};

            var types = new Stack<Type>();

            types.Push(type);
            var finalTypes = new List<Type>();

            while (types.Count > 0)
            {
                var cType = types.Pop();
                finalTypes.Add(cType);

                if (cType.IsGenericType)
                    foreach (var argType in cType.GetGenericArguments())
                        types.Push(argType);
            }

            return finalTypes.ToArray();
        }

        private static string GetFriendlyTypeName(Type type)
        {
            if (type.IsGenericParameter) return type.Name;

            if (!type.IsGenericType) return type.FullName;

            var builder = new StringBuilder();
            var name = type.Name;
            var index = name.IndexOf("`", StringComparison.Ordinal);
            builder.AppendFormat("{0}.{1}", type.Namespace, name.Substring(0, index));
            builder.Append('<');
            var first = true;
            foreach (var arg in type.GetGenericArguments())
            {
                if (!first) builder.Append(',');
                builder.Append(GetFriendlyTypeName(arg));
                first = false;
            }

            builder.Append('>');
            return builder.ToString();
        }

        public static string RemapPrimitiveTypes(string typeName)
        {
            switch (typeName.ToLowerInvariant())
            {
                case "short":
                    return "System.Int16";
                case "int":
                    return "System.Int32";
                case "long":
                    return "System.Int64";
                case "ushort":
                    return "System.UInt16";
                case "uint":
                    return "System.UInt32";
                case "ulong":
                    return "System.UInt64";
                case "string":
                    return "System.String";
                case "char":
                    return "System.Char";
                case "boolean":
                case "bool":
                case "bit":
                    return "System.Boolean";
                case "float":
                    return "System.Single";
                case "double":
                    return "System.Double";
                case "decimal":
                case "money":
                    return "System.Decimal";
                case "guid":
                    return "System.Guid";
            }

            return typeName;
        }

        public static Type GetType(string typeName)
        {
            switch (typeName)
            {
                case "System.Int16":
                    return typeof(short?);
                case "System.Int32":
                    return typeof(int?);
                case "System.Int64":
                    return typeof(long?);
                case "System.UInt16":
                    return typeof(ushort?);
                case "System.UInt32":
                    return typeof(uint?);
                case "System.UInt64":
                    return typeof(ulong?);
                case "System.String":
                    return typeof(string);
                case "System.Char":
                    return typeof(char?);
                case "System.Boolean":
                    return typeof(bool?);
                case "System.Single":
                    return typeof(float?);
                case "System.Double":
                    return typeof(double?);
                case "System.Decimal":
                    return typeof(decimal?);
                case "System.Guid":
                    return typeof(Guid?);
            }

            return Type.GetType(typeName);
        }
    }
 }