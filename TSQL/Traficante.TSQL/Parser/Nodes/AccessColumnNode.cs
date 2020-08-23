using System;
using Traficante.TSQL.Evaluator.Visitors;

namespace Traficante.TSQL.Parser.Nodes
{
    public class AccessFieldNode : IdentifierNode
    {
        private Type _returnType;

        public AccessFieldNode(string column, string alias, TextSpan span)
            : this(column, alias, typeof(void), span)
        {
            Id = $"{nameof(AccessFieldNode)}{column}";
        }

        public AccessFieldNode(string column, string alias, Type returnType, TextSpan span)
            : base(column)
        {
            Alias = alias;
            Span = span;
            _returnType = returnType;
            Id = $"{nameof(AccessFieldNode)}{column}{returnType.Name}";
        }

        public string Alias { get; }

        public TextSpan Span { get; }

        public override Type ReturnType => _returnType;

        public override string Id { get; }

        public override void Accept(IExpressionVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override string ToString()
        {
            if (string.IsNullOrEmpty(Alias))
                return Name;
            return $"{Alias}.{Name}";
        }

        new public void ChangeReturnType(Type returnType)
        {
            _returnType = returnType;
        }
    }
}