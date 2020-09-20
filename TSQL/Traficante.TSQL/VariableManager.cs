using System;
using System.Collections.Generic;
using System.Linq;
using Traficante.TSQL.Evaluator.Helpers;
using Traficante.TSQL.Schema.DataSources;

namespace Traficante.TSQL
{
    public class VariableManager
    {
        public List<Variable> Variables { get; set; } = new List<Variable>();

        public void SetVariable<T>(string name, T value)
        {
            SetVariable(name, typeof(T), value);
        }

        public void SetVariable(string name, Type type, object value)
        {
            var variable = Variables.FirstOrDefault(x => string.Equals(x.Name, name, StringComparison.CurrentCultureIgnoreCase));
            if (variable != null)
            {
                variable.Value = value;
            }
            else
            {
                Variables.Add(new Variable(
                    name, 
                    type.IsValueType ? type.MakeNullableType() : type, 
                    value));
            }
        }

        public Variable GetVariable(string name)
        {
            return Variables.FirstOrDefault(x => string.Equals(x.Name, name, StringComparison.CurrentCultureIgnoreCase));
        }
    }

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
