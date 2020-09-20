using System;

namespace Traficante.Connect
{
    public class QueryLanguage
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public static QueryLanguage TraficantSQL = new QueryLanguage("TraficantSQL", "Traficant SQL", "Cross Database Query Language");
        public static QueryLanguage SqlServerSQL = new QueryLanguage("SqlServerSQL", "SqlServer SQL", "SqlServer Query Language");
        public static QueryLanguage MySQLSQL = new QueryLanguage("MySQLSQL", "MySQL SQL", "MySQL Query Language");
        public static QueryLanguage SqliteSQL = new QueryLanguage("SqliteSQL", "Sqlite SQL", "Sqlite Query Language");
        public static QueryLanguage ElasticSearchDSL = new QueryLanguage("ElasticSearchDSL", "ElasticSearch DSL", "ElasticSearch Query Language");

        public static QueryLanguage[] All => new[] { TraficantSQL, SqlServerSQL, MySQLSQL, SqliteSQL };

        public QueryLanguage()
        { }
        public QueryLanguage(string id, string name, string description)
        {
            Id = id;
            Name = name;
            Description = description;
        }

        public override bool Equals(object obj)
        {
            return (obj as QueryLanguage)?.Id == this.Id;
        }

        public override int GetHashCode()
        {
            return this.Id.GetHashCode();
        }

    }
}
