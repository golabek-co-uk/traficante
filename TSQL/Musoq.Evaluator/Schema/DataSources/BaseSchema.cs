using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Traficante.TSQL.Plugins.Attributes;
using Traficante.TSQL.Schema.Helpers;
using Traficante.TSQL.Schema.Managers;
using Traficante.TSQL.Schema.Reflection;

namespace Traficante.TSQL.Schema.DataSources
{
    public abstract class BaseDatabase : IDatabase
    {
        //private const string _sourcePart = "_source";
        //private const string _tablePart = "_table";

        private readonly MethodsAggregator _aggregator;

        private IDictionary<string, Reflection.ConstructorInfo[]> Constructors { get; } = new Dictionary<string, Reflection.ConstructorInfo[]>();
        private List<SchemaMethodInfo> ConstructorsMethods { get; } = new List<SchemaMethodInfo>();
        private IDictionary<string, object[]> AdditionalArguments { get; } = new Dictionary<string, object[]>();

        public string Name { get; set; }
        public string DefaultSchema { get; set; }
        public List<(ITable Table, RowSource Source)> Tables { get; private set; }
        public List<(ITable Table, RowSource Source)> Functions { get; private set; }


        protected BaseDatabase(string name, MethodsAggregator methodsAggregator)
        {
            Name = name;
            Tables = new List<(ITable Table, RowSource Source)>();
            Functions = new List<(ITable Table, RowSource Source)>();
            _aggregator = methodsAggregator;
        }

        protected BaseDatabase(string name, string defaultSchema, MethodsAggregator methodsAggregator)
        {
            Name = name;
            DefaultSchema = defaultSchema;
            Tables = new List<(ITable Table, RowSource Source)>();
            Functions = new List<(ITable Table, RowSource Source)>();
            _aggregator = methodsAggregator;
        }

        public void AddTable<TType>(string schema, string name, IEnumerable<TType> items)
        {
            var entityMap = TypeHelper.GetEntityMap<TType>();
            Tables.Add((new SchemaTable(schema ?? DefaultSchema, name, entityMap.Columns), new EntitySource<TType>(entityMap, items)));
        }

        public void AddFunction<TType>(string schema, string name, Func<IEnumerable<TType>> items)
        {
            var entityMap = TypeHelper.GetEntityMap<TType>();
            Functions.Add((new SchemaTable(schema ?? DefaultSchema, name, entityMap.Columns), new EntitySource<TType>(entityMap, items())));
        } 

        public virtual ITable GetTableByName(string schema, string name)
        {
            return Tables.FirstOrDefault(x => 
                    string.Equals(x.Table.Schema, schema ?? DefaultSchema, StringComparison.CurrentCultureIgnoreCase) && 
                    string.Equals(x.Table.Name, name, StringComparison.CurrentCultureIgnoreCase))
                .Table;
        }

        public virtual RowSource GetTableRowSource(string schema, string name)
        {
            return Tables.FirstOrDefault(x =>
                    string.Equals(x.Table.Schema, schema ?? DefaultSchema, StringComparison.CurrentCultureIgnoreCase) &&
                    string.Equals(x.Table.Name, name, StringComparison.CurrentCultureIgnoreCase))
                .Source;
        }

        public virtual ITable GetFunctionByName(string schema, string name, params object[] parameters)
        {
            return Functions.FirstOrDefault(x =>
                    string.Equals(x.Table.Schema, schema ?? DefaultSchema, StringComparison.CurrentCultureIgnoreCase) &&
                    string.Equals(x.Table.Name, name, StringComparison.CurrentCultureIgnoreCase))
                .Table;
        }

        public virtual RowSource GetFunctionRowSource(string schema, string name, object[] parameters)
        {
            return Functions.FirstOrDefault(x =>
                    string.Equals(x.Table.Schema, schema ?? DefaultSchema, StringComparison.CurrentCultureIgnoreCase) &&
                    string.Equals(x.Table.Name, name, StringComparison.CurrentCultureIgnoreCase))
                .Source;
        }

        //private void AddToConstructors<TType>(string name)
        //{
        //    var schemaMethodInfos = TypeHelper
        //        .GetSchemaMethodInfosForType<TType>(name);

        //    ConstructorsMethods.AddRange(schemaMethodInfos);

        //    var schemaMethods = schemaMethodInfos
        //        .Select(schemaMethod => schemaMethod.ConstructorInfo)
        //        .ToArray();

        //    Constructors.Add(name, schemaMethods);
        //}

        //public virtual RowSource GetRowSource(string schema, string name, RuntimeContext interCommunicator, params object[] parameters)
        //{
        //    var sourceName = $"{schema.ToLowerInvariant()}_{name.ToLowerInvariant()}{_sourcePart}";

        //    var methods = GetConstructors(schema, sourceName).Select(c => c.ConstructorInfo).ToArray();

        //    if (AdditionalArguments.ContainsKey(sourceName))
        //        parameters = parameters.ExpandParameters(AdditionalArguments[sourceName]);

        //    if (!TryMatchConstructorWithParams(methods, parameters, out var constructorInfo))
        //        throw new NotSupportedException($"Unrecognized method {name}.");

        //    if (constructorInfo.SupportsInterCommunicator)
        //        parameters = parameters.ExpandParameters(interCommunicator);

        //    return (RowSource)constructorInfo.OriginConstructor.Invoke(parameters);
        //}

        //public SchemaMethodInfo[] GetConstructors(string schema, string methodName)
        //{
        //    return GetConstructors(schema).Where(constr => constr.MethodName == methodName).ToArray();
        //}

        //public virtual SchemaMethodInfo[] GetConstructors(string schema)
        //{
        //    return ConstructorsMethods.ToArray();
        //}

        //public SchemaMethodInfo[] GetRawConstructors(string schema)
        //{
        //    return ConstructorsMethods
        //        .Where(cm => cm.MethodName.Contains(_tablePart))
        //        .Select(cm => {
        //            var index = cm.MethodName.IndexOf(_tablePart);
        //            var rawMethodName = cm.MethodName.Substring(0, index);
        //            return new SchemaMethodInfo(rawMethodName, cm.ConstructorInfo);
        //        }).ToArray();
        //}

        //public SchemaMethodInfo[] GetRawConstructors(string schema, string methodName)
        //{
        //    return GetRawConstructors(schema).Where(constr => constr.MethodName == methodName).ToArray();
        //}

        public bool TryResolveAggreationMethod(string method, Type[] parameters, out MethodInfo methodInfo)
        {
            var founded = _aggregator.TryResolveMethod(method, parameters, out methodInfo);
            return founded;

            if (founded)
                return methodInfo.GetCustomAttribute<AggregationMethodAttribute>() != null;

            return false;
        }

        public MethodInfo ResolveMethod(string schema, string method, Type[] parameters)
        {
            return _aggregator.ResolveMethod(method, parameters);
        }

        //protected bool ParamsMatchConstructor(Reflection.ConstructorInfo constructor, object[] parameters)
        //{
        //    bool matchingResult = true;

        //    if (parameters.Length != constructor.Arguments.Length)
        //        return false;

        //    for (int i = 0; i < parameters.Length && matchingResult; ++i)
        //    {
        //        matchingResult &= 
        //            constructor.Arguments[i].Type.IsAssignableFrom(
        //                parameters[i].GetType());
        //    }

        //    return matchingResult;
        //}

        //protected bool TryMatchConstructorWithParams(Reflection.ConstructorInfo[] constructors, object[] parameters, out Reflection.ConstructorInfo foundedConstructor)
        //{
        //    foreach(var constructor in constructors)
        //    {
        //        if(ParamsMatchConstructor(constructor, parameters))
        //        {
        //            foundedConstructor = constructor;
        //            return true;
        //        }
        //    }

        //    foundedConstructor = null;
        //    return false;
        //}
    }

    public class SchemaTable : ITable
    {
        public SchemaTable(string schema, string name, IColumn[] columns)
        {
            Schema = schema;
            Name = name;
            Columns = columns;
        }

        public IColumn[] Columns { get; }

        public string Name { get; }

        public string Schema { get; }
    }
}