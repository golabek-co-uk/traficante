using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Linq.Expressions;
using System.Text.Json;

namespace Traficante.TSQL.Evaluator.Helpers
{
    public static class TypeHelper
    {
        private static readonly Dictionary<Type, Type> StandarTypes = new Dictionary<Type, Type>
        {
            { typeof(bool?), typeof(bool?)},
            { typeof(byte?), typeof(byte?)},
            { typeof(short?), typeof(short?)},
            { typeof(int?), typeof(int?)},
            { typeof(long?), typeof(long?)},
            { typeof(DateTimeOffset?), typeof(DateTimeOffset?)},
            { typeof(DateTime?), typeof(DateTime?)},
            { typeof(string), typeof(string)},
            { typeof(decimal?), typeof(decimal?)},
            { typeof(double?), typeof(double?)},
            { typeof(float?), typeof(float?)},
        };

        private static readonly Dictionary<Type, Type[]> SafeTypeCompatibilityTable = new Dictionary<Type, Type[]>
            {
                {typeof(bool), new[] {typeof(bool)}},
                {typeof(short), new[] {typeof(short), typeof(byte), typeof(bool)}},
                {typeof(int), new[] {typeof(int), typeof(short), typeof(byte), typeof(bool)}},
                {typeof(long), new[] {typeof(long), typeof(int), typeof(short), typeof(byte), typeof(bool)}},
                {typeof(DateTimeOffset), new[] {typeof(DateTimeOffset)}},
                {typeof(DateTime), new[] {typeof(DateTime)}},
                {typeof(string), new[] {typeof(string)}},
                {typeof(decimal), new[] {typeof(decimal), typeof(double), typeof(float)}},
                {typeof(double), new[] { typeof(double), typeof(float)}},
                {typeof(float), new[] { typeof(double), typeof(float)}},
            };

        private static readonly Dictionary<Type, Type[]> UnsafeTypeCompatibilityTable = new Dictionary<Type, Type[]>
            {
                {typeof(byte), new[] {typeof(long), typeof(int), typeof(short)}},
                {typeof(short), new[] {typeof(long), typeof(int)}},
                {typeof(int), new[] { typeof(long) }},
                {typeof(float), new[] {typeof(decimal) }},
                {typeof(double), new[] {typeof(decimal) }}
            };

        private static readonly IReadOnlyDictionary<(Type, Type), Type> BinaryTypes = new Dictionary<(Type, Type), Type>
            {
                { (typeof(decimal?), typeof(decimal?)), typeof(decimal?)},
                { (typeof(decimal?), typeof(long?)), typeof(decimal?)},
                { (typeof(decimal?), typeof(int?)), typeof(decimal?)},
                { (typeof(decimal?), typeof(short?)), typeof(decimal?)},
                { (typeof(decimal?), typeof(byte?)), typeof(decimal?)},
                { (typeof(decimal?), typeof(string)), typeof(decimal?)},
                { (typeof(decimal?), typeof(JsonElement?)), typeof(decimal?)},

                { (typeof(long?), typeof(decimal?)), typeof(decimal?)},
                { (typeof(long?), typeof(long?)), typeof(long?)},
                { (typeof(long?), typeof(int?)), typeof(long?)},
                { (typeof(long?), typeof(short?)), typeof(long?)},
                { (typeof(long?), typeof(byte?)), typeof(long?)},
                { (typeof(long?), typeof(string)), typeof(long?)},
                { (typeof(long?), typeof(JsonElement?)), typeof(long?)},

                { (typeof(int?), typeof(decimal?)), typeof(decimal?)},
                { (typeof(int?), typeof(long?)), typeof(long?)},
                { (typeof(int?), typeof(int?)), typeof(int?)},
                { (typeof(int?), typeof(short?)), typeof(int?)},
                { (typeof(int?), typeof(byte?)), typeof(int?)},
                { (typeof(int?), typeof(string)), typeof(int?)},
                { (typeof(int?), typeof(JsonElement?)), typeof(int?)},

                { (typeof(short?), typeof(decimal?)), typeof(decimal?)},
                { (typeof(short?), typeof(long?)), typeof(long?)},
                { (typeof(short?), typeof(int?)), typeof(int?)},
                { (typeof(short?), typeof(short?)), typeof(short?)},
                { (typeof(short?), typeof(byte?)), typeof(short?)},
                { (typeof(short?), typeof(string)), typeof(short?)},
                { (typeof(short?), typeof(JsonElement?)), typeof(short?)},

                { (typeof(byte?), typeof(decimal?)), typeof(decimal?)},
                { (typeof(byte?), typeof(long?)), typeof(long?)},
                { (typeof(byte?), typeof(int?)), typeof(int?)},
                { (typeof(byte?), typeof(short?)), typeof(short?)},
                { (typeof(byte?), typeof(byte?)), typeof(byte?)},
                { (typeof(byte?), typeof(string)), typeof(byte?)},
                { (typeof(byte?), typeof(JsonElement?)), typeof(byte?)},

                { (typeof(string), typeof(string)), typeof(string)},
                { (typeof(string), typeof(decimal?)), typeof(decimal?)},
                { (typeof(string), typeof(long?)), typeof(long?)},
                { (typeof(string), typeof(int?)), typeof(int?)},
                { (typeof(string), typeof(short?)), typeof(short?)},
                { (typeof(string), typeof(byte?)), typeof(byte?)},
                { (typeof(string), typeof(bool?)), typeof(bool?)},
                { (typeof(string), typeof(DateTimeOffset?)), typeof(DateTimeOffset?)},
                { (typeof(string), typeof(DateTime?)), typeof(DateTime?)},
                { (typeof(string), typeof(JsonElement?)), typeof(string)},

                { (typeof(JsonElement?), typeof(string)), typeof(string)},
                { (typeof(JsonElement?), typeof(decimal?)), typeof(decimal?)},
                { (typeof(JsonElement?), typeof(long?)), typeof(long?)},
                { (typeof(JsonElement?), typeof(int?)), typeof(int?)},
                { (typeof(JsonElement?), typeof(short?)), typeof(short?)},
                { (typeof(JsonElement?), typeof(byte?)), typeof(byte?)},
                { (typeof(JsonElement?), typeof(bool?)), typeof(bool?)},
                { (typeof(JsonElement?), typeof(DateTimeOffset?)), typeof(DateTimeOffset?)},
                { (typeof(JsonElement?), typeof(DateTime?)), typeof(DateTime?)},
                { (typeof(JsonElement?), typeof(JsonElement?)), typeof(JsonElement?)},

                { (typeof(bool?), typeof(bool?)), typeof(bool?)},
                { (typeof(bool?), typeof(string)), typeof(bool?)},
                { (typeof(bool?), typeof(JsonElement?)), typeof(bool?)},

                { (typeof(DateTimeOffset?), typeof(DateTimeOffset?)), typeof(DateTimeOffset?)},
                { (typeof(DateTimeOffset?), typeof(DateTime?)), typeof(DateTimeOffset?)},
                { (typeof(DateTimeOffset?), typeof(string)), typeof(DateTimeOffset?)},
                { (typeof(DateTimeOffset?), typeof(JsonElement?)), typeof(DateTimeOffset?)},

                { (typeof(DateTime?), typeof(DateTime?)), typeof(DateTime?)},
                { (typeof(DateTime?), typeof(DateTimeOffset?)), typeof(DateTimeOffset?)},
                { (typeof(DateTime?), typeof(string)), typeof(DateTime?)},
                { (typeof(DateTime?), typeof(JsonElement?)), typeof(DateTime?)},

                { (typeof(object), typeof(object)), typeof(object)}
            };


        public static bool IsSafeTypePossibleToConvert(Type to, Type from)
        {
            if (to.IsGenericType)
                to = to.GetUnderlyingNullable();
            if (from.IsGenericType)
                from = from.GetUnderlyingNullable();
            if (SafeTypeCompatibilityTable.ContainsKey(to))
                return SafeTypeCompatibilityTable[to].Any(f => f == from);
            return to == from || to.IsAssignableFrom(from);
        }

        public static bool IsUnsafeTypePossibleToConvert(Type to, Type from)
        {
            if (to.IsGenericType)
                to = to.GetUnderlyingNullable();
            if (from.IsGenericType)
                from = from.GetUnderlyingNullable();
            if (UnsafeTypeCompatibilityTable.ContainsKey(to))
                return UnsafeTypeCompatibilityTable[to].Any(f => f == from);
            return to == from || to.IsAssignableFrom(from);
        }

        static public Type GetReturnType(Type left, Type right)
        {
            if (left == right && StandarTypes.ContainsKey(left))
                return left;

            BinaryTypes.TryGetValue((left, right), out Type returnType);
            if (returnType != null)
                return returnType;

            if (left == typeof(string) || right == typeof(string))
                return typeof(string);

            if (left == typeof(JsonElement?) || right == typeof(JsonElement?))
                return typeof(string);

            if (left == typeof(object) || right == typeof(object))
                return typeof(object);

            throw new TSQLException($"Cannot find commont type for {left.Name} and {right.Name}");
        }

 
        static public bool IsNullable(this Type type)
        {
            return type.Name == "Nullable`1";
        }

        static public Type GetUnderlyingNullable(this Type type)
        {
            var nullableType = Nullable.GetUnderlyingType(type);

            var isNullableType = nullableType != null;

            return isNullableType ? nullableType : type;
        }

        public static Type MakeNullableType(this Type type)
        {
            if (type.IsGenericType == false)
                return typeof(Nullable<>).MakeGenericType(type);
            else
                return type;
        }

        public static int CountOptionalParameters(this ParameterInfo[] parameters)
        {
            return parameters.Count(f => f.IsOptional);
        }

        public static bool HasParameters(this ParameterInfo[] paramsParameters)
        {
            return paramsParameters != null && paramsParameters.Length > 0;
        }

        public static int CountWithoutParametersAnnotatedBy<TType>(this ParameterInfo[] parameters)
            where TType : Attribute
        {
            return parameters.Count(f => f.GetCustomAttribute<TType>() == null);
        }

        public static ParameterInfo[] GetParametersWithAttribute<TType>(this ParameterInfo[] parameters)
            where TType : Attribute
        {
            return parameters.Where(f => f.GetCustomAttribute<TType>() != null).ToArray();
        }

        public static bool IsNumericType(this Type type)
        {
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Byte:
                case TypeCode.SByte:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.Decimal:
                case TypeCode.Double:
                case TypeCode.Single:
                    return true;
                default:
                    return false;
            }
        }

        public static object Default(this Type type)
        {
            if (type.IsValueType)
                return Activator.CreateInstance(type);
            else
                return null;
        }
    }
    
}