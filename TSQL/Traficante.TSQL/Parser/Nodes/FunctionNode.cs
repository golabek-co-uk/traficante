using System;
using System.Collections.Generic;
using System.Reflection;
using Traficante.TSQL.Parser.Tokens;
using Traficante.TSQL.Lib.Attributes;
using System.Linq;

namespace Traficante.TSQL.Parser.Nodes
{
    public class FunctionNode : Node
    {
        public FunctionNode(string name, ArgsListNode args, string[] path, Traficante.TSQL.Schema.Managers.MethodInfo method)
        {
            Name = name;
            Arguments = args;
            Method = method;
            Path = path;
            Id = $"{nameof(FunctionNode)}.{string.Join(".", Path)}{(Path.Length > 0 ? "." : "")}{name}{args.Id}";
        }

        public Traficante.TSQL.Schema.Managers.MethodInfo Method { get; private set; }
        public ArgsListNode Arguments { get; }

        public Type[] ArgumentsTypes => Arguments.Args.Select(x => x.ReturnType).ToArray();

        public string[] Path { get; }
        public string Name { get; }

        public bool IsAggregateMethod =>
            Method != null && Method.FunctionMethod.GetCustomAttribute<AggregationMethodAttribute>() != null;

        public int ArgsCount => Arguments.Args.Length;

        public override Type ReturnType => Method != null ? ResolveGenericMethodReturnType() : typeof(void);

        public override string Id { get; }

        public override void Accept(IExpressionVisitor visitor)
        {
            visitor.Visit(this);
        }

        private Type ResolveGenericMethodReturnType()
        {
            if (!Method.FunctionMethod.ReturnType.IsGenericParameter)
                return Method.FunctionMethod.ReturnType;

            int paramIndex = 0;
            var types = new List<Type>();

            foreach (var param in Method.FunctionMethod.GetParameters())
            {
                if (param.ParameterType.IsGenericParameter && Method.FunctionMethod.ReturnType == param.ParameterType)
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

        public void ChangeMethod(Traficante.TSQL.Schema.Managers.MethodInfo method)
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