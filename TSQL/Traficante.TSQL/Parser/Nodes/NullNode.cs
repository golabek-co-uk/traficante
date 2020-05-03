using System;
using System.Globalization;
using Traficante.TSQL.Evaluator.Visitors;

namespace Traficante.TSQL.Parser.Nodes
{
    public class NullNode : Node
    {
        public NullNode()
        {
            Id = $"{nameof(NullNode)}{ReturnType.Name}";
        }

        public bool Value { get; }


        public override Type ReturnType => typeof(object);

        public override string Id { get; }

        public override string ToString()
        {
            return Value.ToString(CultureInfo.InvariantCulture);
        }

        public override void Accept(IExpressionVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}