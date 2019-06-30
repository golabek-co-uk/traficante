using Musoq.Schema;

namespace Musoq.Evaluator.Tables
{
    internal class VariableTable : ITable
    {
        public VariableTable(IColumn[] columns)
        {
            Columns = columns;
        }

        public IColumn[] Columns { get; }
    }
}