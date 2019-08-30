using System;

namespace Traficante.TSQL.Parser.Nodes
{
    public class SetNode : Node
    {

        public SetNode(VariableNode variable, Node value)
        {
            Variable = variable;
            Value = value;
        }

        public VariableNode Variable { get; }
        public Node Value { get; }

        public override Type ReturnType => typeof(void);

        public override string Id => $"{nameof(SetNode)}{Variable.Id}{Value.Id}";

        public override void Accept(IExpressionVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override string ToString()
        {
            return $"set {Variable} = {Value}";
        }
    }
}
