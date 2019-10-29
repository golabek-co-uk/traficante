using System;
using System.Collections.Generic;
using System.Reflection;
using Traficante.TSQL.Parser.Tokens;
using Traficante.TSQL.Lib.Attributes;

namespace Traficante.TSQL.Parser.Nodes
{
    public class FunctionNode : Node
    {
        public FunctionNode(string name, ArgsListNode args, string[] path,
            MethodInfo method = (MethodInfo) null)
        {
            Name = name;
            Arguments = args;
            Method = method;
            Path = path;
            Id = $"{nameof(FunctionNode)}.{string.Join(".", Path)}{(Path.Length > 0 ? "." : "")}.{name}{args.Id}";
        }

        public MethodInfo Method { get; private set; }

        public ArgsListNode Arguments { get; }

        public string[] Path { get; }
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
            return ArgsCount > 0 ? 
                $"{string.Join(".", Path)}{(Path.Length > 0 ? "." : "")}{Name}({Arguments.ToString()})" : 
                $"{string.Join(".", Path)}{(Path.Length > 0 ? "." : "")}{Name}()";
        }
    }
}