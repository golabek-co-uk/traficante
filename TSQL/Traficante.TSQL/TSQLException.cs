using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Traficante.TSQL
{
    public class TSQLException : Exception
    {
        public TSQLException()
        {
        }

        public TSQLException(string message) : base(message)
        {
        }

        public TSQLException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected TSQLException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
