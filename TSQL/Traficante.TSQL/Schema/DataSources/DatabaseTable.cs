namespace Traficante.TSQL.Schema.DataSources
{
    public class DatabaseTable : ITable
    {
        public DatabaseTable(string name, string[] path, IColumn[] columns)
        {
            Path = Path;
            Name = name;
            Columns = columns;
        }

        public IColumn[] Columns { get; }

        public string Name { get; }

        public string[] Path { get; }
    }
}