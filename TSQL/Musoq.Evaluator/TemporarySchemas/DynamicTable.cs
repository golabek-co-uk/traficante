using Musoq.Schema;

namespace Musoq.Evaluator.TemporarySchemas
{
    public class DynamicTable : ITable
    {
        public DynamicTable(IColumn[] columns)
        {
            Columns = columns;
        }

        public IColumn[] Columns { get; }
    }
}