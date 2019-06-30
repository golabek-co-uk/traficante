using System;

namespace Musoq.Schema
{
    public interface IColumn
    {
        string ColumnName { get; }
        int ColumnIndex { get; }
        Type ColumnType { get; }
    }
}