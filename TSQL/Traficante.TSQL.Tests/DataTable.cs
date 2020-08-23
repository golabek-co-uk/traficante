using Traficante.TSQL.Schema;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Traficante.TSQL.Tests
{
    public class DataTable : IEnumerable<DataRow>
    {
        private readonly Dictionary<int, DataColumn> _columnsByIndex;
        private readonly Dictionary<string, DataColumn> _columnsByName;
        protected internal readonly List<DataRow> Rows;

        public DataTable(string name, DataColumn[] columns)
        {
            Name = name;
            _columnsByIndex = new Dictionary<int, DataColumn>();
            _columnsByName = new Dictionary<string, DataColumn>();
            Rows = new List<DataRow>();

            AddColumns(columns);
        }

        public DataTable(object result)
        {
            Name = "entities";
            _columnsByIndex = new Dictionary<int, DataColumn>();
            _columnsByName = new Dictionary<string, DataColumn>();
            Rows = new List<DataRow>();

            if (result is System.Collections.IEnumerable enumerableResult)
            {
                var itemType = result.GetType().GenericTypeArguments.FirstOrDefault();

                List<DataColumn> columns2 = new List<DataColumn>();
                int index = 0;
                foreach (var field in itemType.GetFields())
                {
                    columns2.Add(new DataColumn(field.Name, field.FieldType, index));
                    index++;
                }
                AddColumns(columns2.ToArray());
                foreach (var row in enumerableResult)
                {
                    object[] values = new object[columns2.Count];
                    for (int i = 0; i < columns2.Count; i++)
                    {
                      values[i] = itemType.GetField(columns2[i].ColumnName).GetValue(row);
                    }
                    DataRow row2 = new DataRow(values);
                    Add(row2);
                }
            }
        }

        public string Name { get; }

        public IEnumerable<DataColumn> Columns => _columnsByIndex.Values;

        public IEnumerator<DataRow> CurrentEnumerator { get; private set; }

        //IReadOnlyList<IReadOnlyRow> IReadOnlyTable.Rows => Rows;

        public DataRow this[int index] => Rows[index];

        public int Count => Rows.Count;

        public IEnumerator<DataRow> GetEnumerator()
        {
            CurrentEnumerator = Rows.GetEnumerator();
            return CurrentEnumerator;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void AddColumns(params DataColumn[] columns)
        {
            for (var i = 0; i < columns.Length; i++)
            {
                _columnsByIndex.Add(columns[i].ColumnIndex, columns[i]);
                _columnsByName.Add(columns[i].ColumnName, columns[i]);
            }
        }

        public DataColumn GetColumn(string name)
        {
            return _columnsByName[name];
        }

        public void Add(DataRow value)
        {
            var newIndex = Rows.Count;

            if (value.Count != _columnsByIndex.Count)
                throw new NotSupportedException(
                    $"({nameof(Add)}) Current row has {value.Count} values but {_columnsByIndex.Count} required.");

            for (var i = 0; i < value.Count; i++)
            {
                if (value[i] == null)
                    continue;

                var t1 = value[i].GetType();
                var t2 = _columnsByIndex[i].ColumnType;
                if (!t2.IsAssignableFrom(t1))
                    throw new NotSupportedException(
                        $"({nameof(Add)}) Mismatched types. {t2.Name} is not assignable from {t1.Name}");
            }

            Rows.Add(value);
        }
    }

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

    public class DataRow : IEquatable<DataRow>
    {
        private readonly object[] _columns;
        public object this[int columnNumber] => _columns[columnNumber];
        public int Count => _columns.Length;
        public object[] Values => _columns;

        public DataRow(object[] columns)
        {
            _columns = columns;
        }

        public bool Equals(DataRow other)
        {
            if (other == null)
                return false;

            if (other.Count != Count)
                return false;

            var isEqual = true;

            for (var i = 0; i < Count && isEqual; ++i)
                isEqual &= this[i].Equals(other[i]);

            return isEqual;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as DataRow);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

    }
}