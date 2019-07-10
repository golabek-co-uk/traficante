namespace Traficante.TSQL.Parser
{
    public interface IQueryPartAwareExpressionVisitor : IExpressionVisitor
    {
        void SetQueryPart(QueryPart part);
    }
}