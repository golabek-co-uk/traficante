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
    public class Variable
    {
        public Variable(string name, Type type, object value)
        {
            Name = name;
            Type = type;
            Value = value;
        }

        public string Name { get; }

        public Type Type { get; }

        public object Value { get; set; }
    }
}