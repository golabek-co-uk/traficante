using Traficante.TSQL.Evaluator.Utils;
using Traficante.TSQL.Parser;

namespace Traficante.TSQL.Evaluator.Visitors
{
    public interface IScopeAwareExpressionVisitor : IExpressionVisitor
    {
        void SetScope(Scope scope);
    }
}