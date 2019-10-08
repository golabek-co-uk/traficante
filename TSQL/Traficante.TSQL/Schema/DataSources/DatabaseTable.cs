namespace Traficante.TSQL.Schema.DataSources
{
    public class DatabaseTable : ITable
    {
        public DatabaseTable(string schema, string name, IColumn[] columns)
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