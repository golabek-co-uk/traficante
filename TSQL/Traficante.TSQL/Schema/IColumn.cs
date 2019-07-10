using System;

namespace Traficante.TSQL.Schema
{
    public interface IColumn
    {
        string ColumnName { get; }
        int ColumnIndex { get; }
        Type ColumnType { get; }
    }
}