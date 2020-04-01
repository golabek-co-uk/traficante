namespace Traficante.TSQL.Schema.DataSources
{
    public class Table
    {
        public Table(string name, string[] path, Schema.DataSources.Column[] columns)
        {
            Path = Path;
            Name = name;
            Columns = columns;
        }

        public Schema.DataSources.Column[] Columns { get; }

        public string Name { get; }

        public string[] Path { get; }
    }
}