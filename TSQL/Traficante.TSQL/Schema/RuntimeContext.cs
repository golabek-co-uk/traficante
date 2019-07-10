using System;
using System.Collections.Generic;
using System.Threading;

namespace Traficante.TSQL.Schema
{
    public class RuntimeContext
    {
        public CancellationToken EndWorkToken { get; }

        public IReadOnlyCollection<IColumn> AllColumns { get; }

        public IReadOnlyCollection<IColumn> UsedColumns { get; }

        public RuntimeContext(CancellationToken endWorkToken, IReadOnlyCollection<IColumn> originallyInferedColumns, IReadOnlyCollection<IColumn> usedColumns = null)
        {
            EndWorkToken = endWorkToken;
            AllColumns = originallyInferedColumns;
            UsedColumns = usedColumns;
        }

        public static RuntimeContext Empty => new RuntimeContext(CancellationToken.None, new IColumn[0], new IColumn[0]);
    }
}
