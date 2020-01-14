using System;
using Traficante.TSQL.Evaluator.Visitors;

namespace Traficante.TSQL.Parser.Nodes
{
    public class IdentifierNode : Node
    {
        private Type _returnType = null;
        public IdentifierNode(string name, Type returnType = null)
        {
            Name = name;
            _returnType = returnType;
            Id = $"{nameof(IdentifierNode)}{Name}";
        }

        public string Name { get; }

        public override Type ReturnType
        {
            get { return _returnType; }
        }

        public void SetReturnType(Type type)
        {
            _returnType = type;
        }

        public override string Id { get; }

        public override void Accept(IExpressionVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override string ToString()
        {
            return Name;
        }
    }
}