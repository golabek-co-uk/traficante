namespace Traficante.TSQL.Parser.Tokens
{
    public class CommentToken : Token
    {
        public const string TokenText = "--";

        public CommentToken(TextSpan span)
            : base(TokenText, TokenType.Comment, span)
        {
        }
    }
}