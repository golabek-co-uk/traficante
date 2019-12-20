using System;
using System.Diagnostics;

namespace Traficante.TSQL.Schema.DataSources
{
    [DebuggerDisplay("{ColumnType.FullName} {ColumnName}: {ColumnIndex}")]
    public class Column
    {
        public Column(string columnName, int columnIndex, Type columnType)
        {
            ColumnName = columnName;
            ColumnIndex = columnIndex;
            ColumnType = columnType;
        }

        public string ColumnName { get; }
        public int ColumnIndex { get; }
        public Type ColumnType { get; }
    }
}