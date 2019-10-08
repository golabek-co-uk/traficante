using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Traficante.TSQL.Lib;
using Traficante.TSQL.Plugins.Attributes;
using Traficante.TSQL.Schema.Helpers;
using Traficante.TSQL.Schema.Managers;
using Traficante.TSQL.Schema.Reflection;

namespace Traficante.TSQL.Schema.DataSources
{
    public abstract class BaseDatabase : IDatabase
    {
        public string Name { get; set; }
        public string DefaultSchema { get; set; }
        public List<(ITable Table, RowSource Source)> Tables { get; set; } = new List<(ITable Table, RowSource Source)>();
        public List<(ITable Table, RowSource Source)> Functions { get; set; } = new List<(ITable Table, RowSource Source)>();
        public MethodsManager MethodsManager { get; set; } = new MethodsManager();

        protected BaseDatabase(string name)
        {
            Name = name;
            MethodsManager.RegisterLibraries(new Library());
        }

        protected BaseDatabase(string name, string defaultSchema)
        {
            Name = name;
            DefaultSchema = defaultSchema;
            MethodsManager.RegisterLibraries(new Library());
        }

        public void AddTable<TType>(string schema, string name, IEnumerable<TType> items)
        {
            var entityMap = TypeHelper.GetEntityMap<TType>();
            Tables.Add((new DatabaseTable(schema ?? DefaultSchema, name, entityMap.Columns), new EntitySource<TType>(entityMap, items)));
        }

        public void AddFunction<TType>(string schema, string name, Func<IEnumerable<TType>> func)
        {
            var entityMap = TypeHelper.GetEntityMap<TType>();
            this.MethodsManager.RegisterMethod(name, func.Method);
            Functions.Add((new DatabaseTable(schema ?? DefaultSchema, name, entityMap.Columns), new EntitySource<TType>(entityMap, func())));
        }

        public void AddFunction<TResult>(string schema, string name, Func<TResult> func)
        {
            this.MethodsManager.RegisterMethod(name, func.Method);
        }

        public void AddFunction<T1, TResult>(string schema, string name, Func<T1,TResult> func)
        {
            this.MethodsManager.RegisterMethod(name, func.Method);
        }

        public void AddFunction<T1, T2, TResult>(string schema, string name, Func<T1, T2, TResult> func)
        {
            this.MethodsManager.RegisterMethod(name, func.Method);
        }

        public void AddFunction<T1, T2, T3, TResult>(string schema, string name, Func<T1, T2, T3, TResult> func)
        {
            this.MethodsManager.RegisterMethod(name, func.Method);
        }

        public void AddFunction<T1, T2, T3, T4, TResult>(string schema, string name, Func<T1, T2, T3, T4, TResult> func)
        {
            this.MethodsManager.RegisterMethod(name, func.Method);
        }

        public void AddFunction<T1, T2, T3, T4, T5, TResult>(string schema, string name, Func<T1, T2, T3, T4, T5, TResult> func)
        {
            this.MethodsManager.RegisterMethod(name, func.Method);
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

        public bool TryResolveAggreationMethod(string method, Type[] parameters, out MethodInfo methodInfo)
        {
            return MethodsManager.TryGetMethod(method, parameters, out methodInfo);
        }

        public MethodInfo ResolveMethod(string schema, string method, Type[] parameters)
        {
            return MethodsManager.GetMethod(method, parameters);
        }
    }

    public class DatabaseFunction : IFunction
    {
        public DatabaseFunction(string schema, string name, Type returnType, Type[] argumentsTypes)
        {
            Schema = schema;
            Name = name;
            ReturnType = returnType;
            ArgumentsTypes = argumentsTypes;
        }

        public IColumn[] Columns { get; }

        public string Name { get; }

        public string Schema { get; }

        public Type ReturnType { get; set; }

        public Type[] ArgumentsTypes {get; set;}
    }

    public class DatabaseVariable : IVariable
    {
        public DatabaseVariable(string schema, string name, Type type, object value)
        {
            Schema = schema;
            Name = name;
            Type = type;
            Value = value;
        }

        public string Name { get; }

        public string Schema { get; }

        public Type Type { get; }

        public object Value { get; set; }
    }
}