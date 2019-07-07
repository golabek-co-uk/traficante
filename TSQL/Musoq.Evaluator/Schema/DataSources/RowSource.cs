using System.Collections.Generic;

namespace Traficante.TSQL.Schema.DataSources
{
    public abstract class RowSource
    {
        public abstract IEnumerable<IObjectResolver> Rows { get; }
    }
}