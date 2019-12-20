using System;
using System.Collections.Generic;
using System.Threading;
using Traficante.TSQL.Schema.DataSources;

namespace Traficante.TSQL.Schema
{
    public class RuntimeContext
    {
        public CancellationToken EndWorkToken { get; }

        public IReadOnlyCollection<Column> AllColumns { get; }

        public IReadOnlyCollection<Column> UsedColumns { get; }

        public RuntimeContext(CancellationToken endWorkToken, IReadOnlyCollection<Column> originallyInferedColumns, IReadOnlyCollection<Column> usedColumns = null)
        {
            EndWorkToken = endWorkToken;
            AllColumns = originallyInferedColumns;
            UsedColumns = usedColumns;
        }

        public static RuntimeContext Empty => new RuntimeContext(CancellationToken.None, new Column[0], new Column[0]);
    }
}
