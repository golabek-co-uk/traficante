namespace Traficante.TSQL.Parser.Tokens
{
    public class ExecuteToken : Token
    {
        public const string TokenText = "execute";

        public ExecuteToken(TextSpan span)
            : base(TokenText, TokenType.Execute, span)
        {
        }
    }
}