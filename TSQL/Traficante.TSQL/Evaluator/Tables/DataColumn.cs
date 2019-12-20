using Traficante.TSQL.Schema;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Traficante.TSQL.Evaluator.Tables
{
    [DebuggerDisplay("{ColumnIndex}. {ColumnName}: {ColumnType.Name}")]
    public class DataColumn : IEquatable<DataColumn>
    {
        public DataColumn(string name, Type columnType, int columnOrder)
        {
            ColumnName = name;
            ColumnType = columnType;
            ColumnIndex = columnOrder;
        }

        public string ColumnName { get; }

        public Type ColumnType { get; }

        public int ColumnIndex { get; }

        public bool Equals(DataColumn other)
        {
            return other != null &&
                   ColumnName == other.ColumnName &&
                   EqualityComparer<Type>.Default.Equals(ColumnType, other.ColumnType) &&
                   ColumnIndex == other.ColumnIndex;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as DataColumn);
        }

        public override int GetHashCode()
        {
            var hashCode = -1716540554;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(ColumnName);
            hashCode = hashCode * -1521134295 + EqualityComparer<Type>.Default.GetHashCode(ColumnType);
            hashCode = hashCode * -1521134295 + ColumnIndex.GetHashCode();
            return hashCode;
        }
    }
}