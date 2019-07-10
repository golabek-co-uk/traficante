using System;
using System.Collections.Generic;
using System.Text;

namespace Traficante.TSQL.Schema
{
    public interface IReadOnlyTable
    {
        IReadOnlyList<IReadOnlyRow> Rows { get; }

        int Count { get; }
    }
}
