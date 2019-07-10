using Traficante.TSQL.Parser;

namespace Traficante.TSQL.Evaluator.Visitors
{
    public interface IAwareExpressionVisitor : IScopeAwareExpressionVisitor, IQueryPartAwareExpressionVisitor
    {}
}