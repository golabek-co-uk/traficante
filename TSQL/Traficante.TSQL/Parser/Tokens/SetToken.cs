namespace Traficante.TSQL.Parser.Tokens
{
    public class SetToken : Token
    {
        public const string TokenText = "set";

        public SetToken(TextSpan span)
            : base(TokenText, TokenType.Set, span)
        {
        }
    }
}