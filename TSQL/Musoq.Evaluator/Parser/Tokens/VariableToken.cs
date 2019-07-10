namespace Traficante.TSQL.Parser.Tokens
{
    public class VariableToken : Token
    {
        public VariableToken(string value, TextSpan span)
            : base(value, TokenType.Variable, span)
        {
        }
    }
}