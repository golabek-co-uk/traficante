using System;
using System.Reflection;
using Traficante.TSQL.Evaluator.Visitors;

namespace Traficante.TSQL.Parser.Nodes
{
    public class AccessArrayFieldNode : IdentifierNode
    {
        public AccessArrayFieldNode(NumericAccessToken token)
            : base(token.Name)
        {
            Token = token;
            Id = $"{nameof(AccessArrayFieldNode)}{token.Value}";
        }

        public NumericAccessToken Token { get; }

        public string ObjectName => Token.Name;

        public override Type ReturnType => base.ReturnType;

        public override string Id { get; }

        public override void Accept(IExpressionVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override string ToString()
        {
            return $"{ObjectName}[{Token.Index}]";
        }
    }
}