namespace Traficante.TSQL.Parser.Tokens
{
    public class TopToken : Token
    {
        public const string TokenText = "top";

        public TopToken(TextSpan span) : base(TokenText, TokenType.Top, span)
        {
        }
    }
    
}