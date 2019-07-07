using System;

namespace Traficante.TSQL.Parser.Lexing
{
    public class UnknownTokenException : Exception
    {
        public UnknownTokenException(int position, char c, string s)
        {
        }
    }
}