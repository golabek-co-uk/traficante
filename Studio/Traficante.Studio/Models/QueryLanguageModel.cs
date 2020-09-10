namespace Traficante.Studio.ViewModels
{
    public class QueryLanguageModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public static QueryLanguageModel TraficantSQL = new QueryLanguageModel("TraficantSQL", "Traficant SQL", "Cross Database Query Language");
        public static QueryLanguageModel SqlServerSQL = new QueryLanguageModel("SqlServerSQL", "T-SQL", "SqlServer Query Language");
        public static QueryLanguageModel MySQLSQL = new QueryLanguageModel("MySQLSQL", "MySQL SQL", "MySQL Query Language");
        public static QueryLanguageModel SqliteSQL = new QueryLanguageModel("SqliteSQL", "Sqlite SQL", "Sqlite Query Language");
        public static QueryLanguageModel[] All => new[] { TraficantSQL , SqlServerSQL, MySQLSQL, SqliteSQL };

        public QueryLanguageModel()
        { }
        public QueryLanguageModel(string id, string name, string description)
        {
            Id = id;
            Name = name;
            Description = description;
        }

        public override bool Equals(object obj)
        {
            return (obj as QueryLanguageModel)?.Id == this.Id;
        }

        public override int GetHashCode()
        {
            return this.Id.GetHashCode();
        }
    }
}
