using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Traficante.TSQL.Lib;
using Traficante.TSQL.Lib.Attributes;
using Traficante.TSQL.Schema.Helpers;
using Traficante.TSQL.Schema.Managers;
using Traficante.TSQL.Schema.Reflection;

namespace Traficante.TSQL.Schema.DataSources
{

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