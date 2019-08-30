namespace Traficante.TSQL.Parser.Tokens
{
    public class DeclareToken : Token
    {
        public const string TokenText = "declare";

        public DeclareToken(TextSpan span)
            : base(TokenText, TokenType.Declare, span)
        {
        }
    }
}