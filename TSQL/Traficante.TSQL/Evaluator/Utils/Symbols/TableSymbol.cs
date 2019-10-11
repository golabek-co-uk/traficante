using System;
using System.Collections.Generic;
using System.Linq;
using Traficante.TSQL.Evaluator.Tables;
using Traficante.TSQL.Schema;
using Traficante.TSQL.Schema.DataSources;

namespace Traficante.TSQL.Evaluator.Utils.Symbols
{
    public class TableSymbol : Symbol
    {
        private readonly List<string> _orders = new List<string>();

        private readonly Dictionary<string, Tuple<IDatabase, ITable>> _tables =
            new Dictionary<string, Tuple<IDatabase, ITable>>();

        private string _fullTableName;

        private ITable _fullTable;
        private IDatabase _fullSchema;

        public string SchemaName { get; }

        public TableSymbol(string schemaName, string alias, IDatabase schema, ITable table, bool hasAlias)
        {
            _tables.Add(alias, new Tuple<IDatabase, ITable>(schema, table));
            _orders.Add(alias);
            HasAlias = hasAlias;
            SchemaName = schemaName;
            _fullTableName = alias;

            _fullSchema = schema;
            _fullTable = table;
        }

        private TableSymbol()
        {
            HasAlias = true;
        }

        public bool HasAlias { get; }

        public string[] CompoundTables => _orders.ToArray();

        public (IDatabase Schema, ITable Table, string TableName) GetTableByColumnName(string column)
        {
            (IDatabase, ITable, string) score = (null, null, null);

            foreach (var table in _tables)
            {
                var col = table.Value.Item2.Columns.SingleOrDefault(c => c.ColumnName == column);

                if (col == null)
                    throw new NotSupportedException($"Unrecognized column ({column})");

                score = (table.Value.Item1, table.Value.Item2, table.Key);
            }

            return score;
        }

        public (IDatabase Schema, ITable Table, string TableName) GetTableByAlias(string alias)
        {
            if (_fullTableName == alias)
                return (_fullSchema, _fullTable, alias);
            return (_tables[alias].Item1, _tables[alias].Item2, alias);
        }

        public IColumn GetColumnByAliasAndName(string alias, string columnName)
        {
            if (_fullTableName == alias)
                return _fullTable.Columns.Single(c => c.ColumnName == columnName);

            return _tables[alias].Item2.Columns.Single(c => c.ColumnName == columnName);
        }

        public IColumn GetColumn(string columnName)
        {
            IColumn column = null;
            foreach (var table in _orders)
            {
                var tmpColumn = _tables[table].Item2.Columns.SingleOrDefault(col => col.ColumnName == columnName);

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

        public IColumn[] GetColumns(string alias)
        {
            return _tables[alias].Item2.Columns;
        }

        public IColumn[] GetColumns()
        {
            var columns = new List<IColumn>();
            foreach (var table in _orders) columns.AddRange(GetColumns(table));

            return columns.ToArray();
        }

        public int GetColumnIndex(string alias, string columnName)
        {
            var i = 0;
            var count = 0;
            while (_orders[i] != alias)
            {
                count += _tables[_orders[i]].Item2.Columns.Length;
                i++;
            }

            var columns = _tables[_orders[i]].Item2.Columns;
            var j = 0;
            for (; j < columns.Length; j++)
                if (columns[j].ColumnName == columnName)
                    break;

            return count + j + 1;
        }

        public TableSymbol MergeSymbols(TableSymbol other)
        {
            var symbol = new TableSymbol();

            var compundTableColumns = new List<IColumn>();

            foreach (var item in _tables)
            {
                symbol._tables.Add(item.Key, item.Value);
                symbol._orders.Add(item.Key);

                compundTableColumns.AddRange(item.Value.Item2.Columns);
            }

            foreach (var item in other._tables)
            {
                symbol._tables.Add(item.Key, item.Value);
                symbol._orders.Add(item.Key);

                compundTableColumns.AddRange(item.Value.Item2.Columns);
            }

            symbol._fullTableName = symbol._orders.Aggregate((a, b) => a + b);
            symbol._fullTable = new DatabaseTable(symbol.SchemaName, symbol._fullTableName, compundTableColumns.ToArray());
            symbol._fullSchema = new TransitionSchema(symbol._fullTableName, null, symbol._fullTable);

            return symbol;
        }
    }
}