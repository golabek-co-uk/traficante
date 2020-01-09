using Traficante.TSQL.Evaluator.Utils;
using Traficante.TSQL.Parser;

namespace Traficante.TSQL.Evaluator.Visitors
{
    public interface IAwareExpressionVisitor : IExpressionVisitor
    {
        void SetScope(Scope scope);
        void SetQueryPart(QueryPart part);
    }
}