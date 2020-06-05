namespace Traficante.TSQL.Parser.Tokens
{
    public class InToToken : Token
    {
        public const string TokenText = "into";

        public InToToken(TextSpan span)
            : base(TokenText, TokenType.InTo, span)
        {
        }
    }
}