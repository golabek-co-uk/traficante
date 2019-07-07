using Traficante.TSQL.Parser;

namespace Traficante.TSQL.Evaluator.Visitors
{
    public class CloneTraverseVisitor : RawTraverseVisitor<IExpressionVisitor>
    {
        public CloneTraverseVisitor(IExpressionVisitor visitor) 
            : base(visitor)
        {
        }
    }
}