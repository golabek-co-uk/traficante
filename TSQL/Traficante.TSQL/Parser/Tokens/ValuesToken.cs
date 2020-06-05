namespace Traficante.TSQL.Parser.Tokens
{
    public class ValuesToken : Token
    {
        public const string TokenText = "values";

        public ValuesToken(TextSpan span)
            : base(TokenText, TokenType.Values, span)
        {
        }
    }
}