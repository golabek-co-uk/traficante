using System;

namespace Traficante.TSQL.Schema
{
    public interface IFunction
    {
        string Name { get; }
        string Schema { get; }
        Type ReturnType { get; }
        Type[] ArgumentsTypes { get; }
        IColumn[] Columns { get; }
    }
}