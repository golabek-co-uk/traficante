using Traficante.TSQL.Schema;

namespace Traficante.TSQL.Evaluator.TemporarySchemas
{
    public class DynamicTable : ITable
    {
        public DynamicTable(string schema, string name, IColumn[] columns)
        {
            Schema = schema;
            Name = name;
            Columns = columns;
        }

        public IColumn[] Columns { get; }

        public string Name { get; }

        public string Schema { get; }
    }
}