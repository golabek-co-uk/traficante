using System;
using System.Reflection;

namespace Traficante.TSQL.Parser.Nodes
{
    public class VariableNode : IdentifierNode
    {
        public VariableNode(string name)
            : base(name)
        {
        }

        public VariableNode(string name, Type type)
            : base(name, type)
        {
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