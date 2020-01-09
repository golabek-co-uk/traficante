using Traficante.TSQL.Parser;
using Traficante.TSQL.Parser.Nodes;

namespace Traficante.TSQL.Evaluator.Visitors
{
    public class GetSelectFieldsTraverseVisitor : CloneTraverseVisitor
    {
        public GetSelectFieldsTraverseVisitor(IAwareExpressionVisitor visitor) 
            : base(visitor)
        {
        }

    }
}