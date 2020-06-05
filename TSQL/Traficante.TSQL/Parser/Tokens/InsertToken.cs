namespace Traficante.TSQL.Parser.Tokens
{
    public class InsertToken : Token
    {
        public const string TokenText = "insert";

        public InsertToken(TextSpan span)
            : base(TokenText, TokenType.Insert, span)
        {
        }
    }
}