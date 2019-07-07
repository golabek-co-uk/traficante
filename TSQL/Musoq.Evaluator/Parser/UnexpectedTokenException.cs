using System;
using Traficante.TSQL.Parser.Tokens;

namespace Traficante.TSQL.Parser
{
    internal class UnexpectedTokenException<T> : Exception
    {
        public UnexpectedTokenException(int position, Token current)
            : base($"Token {current.TokenType} at position {position} is unexpected.")
        {
        }
    }
}