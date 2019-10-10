using System;
using System.Collections.Generic;
using System.Reflection;
using Traficante.TSQL.Parser.Tokens;
using Traficante.TSQL.Plugins.Attributes;

namespace Traficante.TSQL.Parser.Nodes
{
    public class FunctionNode : Node
    {
        public FunctionNode(string database, string schema, string name, ArgsListNode args,
            MethodInfo method = (MethodInfo) null)
        {
            Name = name;
            Schema = schema;
            Database = database;
            Arguments = args;
            Method = method;
            Id = $"{nameof(FunctionNode)}{database}.{schema}.{name}{args.Id}";
        }

        public MethodInfo Method { get; private set; }

        public ArgsListNode Arguments { get; }

        public string Database { get; }
        public string Schema { get; }
        public string Name { get; }

        public bool IsAggregateMethod =>
            Method != null && Method.GetCustomAttribute<AggregationMethodAttribute>() != null;

        public int ArgsCount => Arguments.Args.Length;

        public override Type ReturnType => Method != null ? ResolveGenericMethodReturnType() : typeof(void);

        public override string Id { get; }

        public override void Accept(IExpressionVisitor visitor)
        {
            visitor.Visit(this);
        }

        private Type ResolveGenericMethodReturnType()
        {
            if (!Method.ReturnType.IsGenericParameter)
                return Method.ReturnType;

            int paramIndex = 0;
            var types = new List<Type>();

            foreach (var param in Method.GetParameters())
            {
                if (param.ParameterType.IsGenericParameter && Method.ReturnType == param.ParameterType)
                {
                    types.Add(Arguments.Args[paramIndex].ReturnType);
                }
                paramIndex += 1;
            }

            return GetTheMostCommonBaseTypes(types.ToArray());
        }

        private Type GetTheMostCommonBaseTypes(Type[] types)
        {
            if (types.Length == 0)
                return typeof(object);

            Type ret = types[0];

            for (int i = 1; i < types.Length; ++i)
            {
                if (types[i].IsAssignableFrom(ret))
                    ret = types[i];
                else
                {
                    // This will always terminate when ret == typeof(object)
                    while (!ret.IsAssignableFrom(types[i]))
                        ret = ret.BaseType;
                }
            }

            return ret;
        }

        public void ChangeMethod(MethodInfo method)
        {
            Method = method;
        }

        public override string ToString()
        {
            return ArgsCount > 0 ? $"{Database}{Schema}{Name}({Arguments.ToString()})" : $"{Name}()";
        }
    }
}