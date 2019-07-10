namespace Traficante.TSQL.Parser.Tokens
{
    public class PropertyToken : Token
    {
        public PropertyToken(string value, TextSpan span)
            : base(value, TokenType.Property, span)
        {
        }
    }
}