using System;

namespace Traficante.TSQL.Schema
{
    public interface IVariable
    {
        string Name { get; }
        string Schema { get; }
        Type Type { get; }
        object Value { get; }
    }
}