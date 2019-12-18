using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Traficante.TSQL.Schema.Attributes;
using Traficante.TSQL.Schema.DataSources;

namespace Traficante.TSQL.Schema.Helpers
{
    public static class TypeHelper
    {

        public static Type GetUnderlyingNullable(this Type type)
        {
            var nullableType = Nullable.GetUnderlyingType(type);

            var isNullableType = nullableType != null;

            return isNullableType ? nullableType : type;
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

        public static EntityMap<TType> GetEntityMap<TType>()
        {
            var columnIndex = 0;

            var nameToIndexMap = new Dictionary<string, int>();
            var indexToMethodAccess = new Dictionary<int, Func<TType, object>>();
            var columns = new List<IColumn>();

            var type = typeof(TType);
            foreach (var member in type.GetMembers())
            {
                if(member.MemberType != MemberTypes.Property)
                    continue;

                var property = (PropertyInfo)member;

                Func<TType, object> del;
                if (property.PropertyType.IsValueType)
                {
                    var dynMethod = new DynamicMethod($"Dynamic_Get_{typeof(TType).Name}_{property.Name}", typeof(object), new[] { typeof(TType) }, typeof(TType).Module);
                    var ilGen = dynMethod.GetILGenerator();
                    ilGen.Emit(OpCodes.Ldarg_0);
                    ilGen.Emit(OpCodes.Callvirt, property.GetGetMethod());
                    ilGen.Emit(OpCodes.Box, property.PropertyType);
                    ilGen.Emit(OpCodes.Ret);

                    del = (Func<TType, object>)dynMethod.CreateDelegate(typeof(Func<TType, object>));
                }
                else
                {
                    del = (Func<TType, object>)Delegate.CreateDelegate(typeof(Func<TType, object>), null, property.GetGetMethod());
                }

                nameToIndexMap.Add(property.Name, columnIndex);
                indexToMethodAccess.Add(columnIndex, (instance) => del(instance));
                columns.Add(new Column(property.Name, columnIndex, property.PropertyType));

                columnIndex += 1;
            }

            return new EntityMap<TType>
            {
                NameToIndexMap = nameToIndexMap,
                IndexToMethodAccessMap = indexToMethodAccess,
                Columns = columns.ToArray()
            };
        }

        public static IColumn[] GetColumns(Type typ)
        {
            var columns = new List<IColumn>();
            Type returnType = null;

            if (typeof(System.Collections.IEnumerable).IsAssignableFrom(typ))
                returnType = typ.GenericTypeArguments.FirstOrDefault();
            else
                returnType = typ;
            var columnIndex = 0;
            foreach (var member in returnType.GetMembers())
            {
                if (member.MemberType != MemberTypes.Property &&
                    member.MemberType != MemberTypes.Field)
                    continue;

                var property = (PropertyInfo)member;
                columns.Add(new Traficante.TSQL.Schema.DataSources.Column(property.Name, columnIndex, property.PropertyType));
                columnIndex += 1;
            }

            return columns.ToArray();
        }
    }

    public class EntityMap<TType>
    {
        public IDictionary<string, int> NameToIndexMap { get; set; }
        public IDictionary<int, Func<TType, object>> IndexToMethodAccessMap { get; set; }
        public IColumn[] Columns { get; set; }
    }
}