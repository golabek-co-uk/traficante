namespace Traficante.TSQL.Parser.Tokens
{
    public class UpdateToken : Token
    {
        public const string TokenText = "update";

        public UpdateToken(TextSpan span)
            : base(TokenText, TokenType.Update, span)
        {
        }
    }
}