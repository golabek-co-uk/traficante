using System;
using System.Reflection;
using Traficante.TSQL.Evaluator.Visitors;

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
        
        public override string Id => $"{nameof(VariableNode)}{Name}";

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