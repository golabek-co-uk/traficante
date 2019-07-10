namespace Traficante.TSQL.Parser.Tokens
{
    public class WhiteSpaceToken : Token
    {
        public const string TokenText = " ";

        public WhiteSpaceToken(TextSpan span)
            : base(TokenText, TokenType.WhiteSpace, span)
        {
        }
    }
}