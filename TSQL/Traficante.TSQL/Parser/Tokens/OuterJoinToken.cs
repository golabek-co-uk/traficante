using Traficante.TSQL.Parser.Nodes;

namespace Traficante.TSQL.Parser.Tokens
{
    public class OuterJoinToken : Token
    {
        public const string TokenText = "outer join";

        public OuterJoinToken(string type, TextSpan span)
            : base(TokenText, TokenType.OuterJoin, span)
        {
            Type = type;
        }

        public string Type { get; }
    }
}