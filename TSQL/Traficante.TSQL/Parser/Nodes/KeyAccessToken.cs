using Traficante.TSQL.Parser.Tokens;

namespace Traficante.TSQL.Parser.Nodes
{
    public class KeyAccessToken : Token
    {
        public KeyAccessToken(string name, string value, TextSpan span)
            : base(value, TokenType.KeyAccess, span)
        {
            Key = value;
            Name = name;
        }

        public string Key { get; }

        public string Name { get; }
    }
}