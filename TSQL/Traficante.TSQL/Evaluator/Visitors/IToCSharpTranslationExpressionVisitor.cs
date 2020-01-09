namespace Traficante.TSQL.Evaluator.Visitors
{
    public interface IToCSharpTranslationExpressionVisitor : IAwareExpressionVisitor
    {
        void SetQueryIdentifier(string identifier);

        void SetMethodAccessType(MethodAccessType type);

        void IncrementMethodIdentifier();
    }
}