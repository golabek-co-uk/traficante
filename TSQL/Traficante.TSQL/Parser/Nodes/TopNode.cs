using System;
using Traficante.TSQL.Evaluator.Visitors;

namespace Traficante.TSQL.Parser.Nodes
{
    public class TopNode : UnaryNode
    {
        public TopNode(IntegerNode expression) : base(expression)
        {
            Id = $"{nameof(SkipNode)}{ReturnType.Name}{Expression.Id}";
            Value = Convert.ToInt64(expression.ObjValue);
        }

        public long Value { get; }

        public override Type ReturnType => typeof(long);

        public override string Id { get; }

        public override void Accept(IExpressionVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override string ToString()
        {
            return $"skip {Expression.ToString()}";
        }
    }
}