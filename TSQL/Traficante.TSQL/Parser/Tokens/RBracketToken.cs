using Traficante.TSQL.Parser.Tokens;

namespace Traficante.TSQL.Parser.Lexing
{
    public class RBracketToken : Token
    {
        public const string TokenText = "}";

        public RBracketToken(TextSpan textSpan)
            : base(TokenText, TokenType.RBracket, textSpan)
        {
        }
    }
}