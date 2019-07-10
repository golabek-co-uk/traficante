using System;

namespace Traficante.TSQL.Schema.Exceptions
{
    public class TableNotFoundException : Exception
    {
        public TableNotFoundException(string table)
            : base(table)
        {
        }
    }
}