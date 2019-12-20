using System;
using System.Collections.Generic;
using System.Linq;
using Traficante.TSQL.Schema;
using Traficante.TSQL.Schema.DataSources;

namespace Traficante.TSQL.Evaluator.Utils.Symbols
{
    public class TableSymbol : Symbol
    {
        private readonly List<string> _orders = new List<string>();

        private readonly Dictionary<string, Table> _tables =
            new Dictionary<string, Table>();

        private string _fullTableName;
        private Table _fullTable;
        
        public string[] Path { get; }

        public TableSymbol(string[] path, string alias, Table table, bool hasAlias)
        {
            _tables.Add(alias, table);
            _orders.Add(alias);
            HasAlias = hasAlias;
            Path = path;
            _fullTableName = alias;
            _fullTable = table;
        }

        private TableSymbol()
        {
            HasAlias = true;
        }

        public bool HasAlias { get; }

        public string[] CompoundTables => _orders.ToArray();

        public (Table Table, string TableName) GetTableByColumnName(string column)
        {
            (Table, string) score = (null, null);

            foreach (var table in _tables)
            {
                var col = table.Value.Columns.SingleOrDefault(c => c.ColumnName == column);

                if (col == null)
                    throw new NotSupportedException($"Unrecognized column ({column})");

                score = (table.Value, table.Key);
            }

            return score;
        }

        public (Table Table, string TableName) GetTableByAlias(string alias)
        {
            if (_fullTableName == alias)
                return (_fullTable, alias);
            return (_tables[alias], alias);
        }

        public Schema.DataSources.Column GetColumnByAliasAndName(string alias, string columnName)
        {
            if (_fullTableName == alias)
                return _fullTable.Columns.Single(c => c.ColumnName == columnName);

            return _tables[alias].Columns.Single(c => c.ColumnName == columnName);
        }

        public Schema.DataSources.Column GetColumn(string columnName)
        {
            Schema.DataSources.Column column = null;
            foreach (var table in _orders)
            {
                var tmpColumn = _tables[table].Columns.SingleOrDefault(col => col.ColumnName == columnName);

                if (column != null)
                    throw new NotSupportedException("Multiple column with the same identifier");

                if (tmpColumn == null)
                    continue;

                column = tmpColumn;
            }

            if (column == null)
                throw new NotSupportedException("No such column.");

            return column;
        }

        public Schema.DataSources.Column[] GetColumns(string alias)
        {
            return _tables[alias].Columns;
        }

        public Schema.DataSources.Column[] GetColumns()
        {
            var columns = new List<Schema.DataSources.Column>();
            foreach (var table in _orders) columns.AddRange(GetColumns(table));

            return columns.ToArray();
        }

        public int GetColumnIndex(string alias, string columnName)
        {
            var i = 0;
            var count = 0;
            while (_orders[i] != alias)
            {
                count += _tables[_orders[i]].Columns.Length;
                i++;
            }

            var columns = _tables[_orders[i]].Columns;
            var j = 0;
            for (; j < columns.Length; j++)
                if (columns[j].ColumnName == columnName)
                    break;

            return count + j + 1;
        }

        public TableSymbol MergeSymbols(TableSymbol other)
        {
            var symbol = new TableSymbol();

            var compundTableColumns = new List<Schema.DataSources.Column>();

            foreach (var item in _tables)
            {
                symbol._tables.Add(item.Key, item.Value);
                symbol._orders.Add(item.Key);

                compundTableColumns.AddRange(item.Value.Columns);
            }

            foreach (var item in other._tables)
            {
                symbol._tables.Add(item.Key, item.Value);
                symbol._orders.Add(item.Key);

                compundTableColumns.AddRange(item.Value.Columns);
            }

            symbol._fullTableName = symbol._orders.Aggregate((a, b) => a + b);
            symbol._fullTable = new Table(symbol._fullTableName, symbol.Path, compundTableColumns.ToArray());

            return symbol;
        }
    }
}