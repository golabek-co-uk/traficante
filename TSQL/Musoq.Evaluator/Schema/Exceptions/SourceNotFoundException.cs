using System;

namespace Traficante.TSQL.Schema.Exceptions
{
    public class SourceNotFoundException : Exception
    {
        public SourceNotFoundException(string table)
            : base(table)
        {
        }
    }
}