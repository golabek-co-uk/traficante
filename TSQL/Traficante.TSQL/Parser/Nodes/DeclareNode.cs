using System;

namespace Traficante.TSQL.Parser.Nodes
{
    public class DeclareNode : Node
    {

        public DeclareNode(VariableNode variable, TypeNode type)
        {
            Variable = variable;
            Type = type;
        }

        public VariableNode Variable { get; }
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
