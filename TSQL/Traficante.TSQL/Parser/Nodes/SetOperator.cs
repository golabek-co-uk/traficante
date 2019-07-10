using Traficante.TSQL.Parser.Tokens;

namespace Traficante.TSQL.Parser.Nodes
{
    public enum SetOperator
    {
        Except = TokenType.Except,
        Union = TokenType.Union,
        Intersect = TokenType.Intersect
    }
}