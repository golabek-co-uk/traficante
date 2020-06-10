using System;
using Traficante.TSQL.Evaluator.Visitors;

namespace Traficante.TSQL.Parser.Nodes
{
    public class UpdateNode : Node
    {
        public override Type ReturnType => throw new NotImplementedException();

        public override string Id => throw new NotImplementedException();

        public override void Accept(IExpressionVisitor visitor)
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            throw new NotImplementedException();
        }
    }
}