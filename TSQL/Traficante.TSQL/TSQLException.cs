using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using Traficante.TSQL.Parser.Tokens;

namespace Traficante.TSQL
{
    public class TSQLException : Exception
    {
        public int? LineNumber { get; private set; }
        public int? ColumnNumber { get; private set; }

        public TSQLException()
        {
        }

        public TSQLException(string message, int? lineNumber = null, int? columnNumber = null) : base(message)
        {
            this.LineNumber = lineNumber;
            this.ColumnNumber = columnNumber;
        }

        public TSQLException(string message, (int? lineNumber, int? columnNumber) position) : base(message)
        {
            this.LineNumber = position.lineNumber;
            this.ColumnNumber = position.columnNumber;
        }

        //public TSQLException(string message, Exception innerException) : base(message, innerException)
        //{
        //}

        //protected TSQLException(SerializationInfo info, StreamingContext context) : base(info, context)
        //{
        //}
    }
}
