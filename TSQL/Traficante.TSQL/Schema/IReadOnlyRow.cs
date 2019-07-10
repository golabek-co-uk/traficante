using System;
using System.Collections.Generic;
using System.Text;

namespace Traficante.TSQL.Schema
{
    public interface IReadOnlyRow
    {
        object this[int columnNumber] { get; }
    }
}
