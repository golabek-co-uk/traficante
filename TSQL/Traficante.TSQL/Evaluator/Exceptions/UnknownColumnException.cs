using System;

namespace Traficante.TSQL.Evaluator.Exceptions
{
    public class UnknownColumnException : Exception
    {
        public UnknownColumnException(string message)
            : base(message)
        {
        }
    }
}