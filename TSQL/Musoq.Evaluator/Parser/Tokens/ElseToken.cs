namespace Traficante.TSQL.Parser.Tokens
{
    public class ElseToken : Token
    {
        public const string TokenText = "else";

        public ElseToken(TextSpan span)
            : base(TokenText, TokenType.Else, span)
        {
        }
    }
}