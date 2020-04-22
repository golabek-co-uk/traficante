using System;
using Traficante.TSQL.Evaluator.Visitors;

namespace Traficante.TSQL.Parser.Nodes
{
    public class DeclareNode : Node
    {

        public DeclareNode(VariableNode variable, TypeNode type, Node value = null)
        {
            Variable = variable;
            Type = type;
            Value = value;
        }

        public VariableNode Variable { get; }
        public Node Value { get; }
        public TypeNode Type { get; }

        public override Type ReturnType => typeof(void);

        public override string Id => $"{nameof(DeclareNode)}{Variable.Id}{Type.Id}";

        public override void Accept(IExpressionVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override string ToString()
        {
            return $"declare {Variable} {Type}";
        }
    }
}
