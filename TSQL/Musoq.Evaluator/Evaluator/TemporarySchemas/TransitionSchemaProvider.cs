using System.Collections.Generic;
using Traficante.TSQL.Schema;

namespace Traficante.TSQL.Evaluator.TemporarySchemas
{
    public class TransitionSchemaProvider : IDatabaseProvider
    {
        private readonly IDatabaseProvider _schemaProvider;
        private readonly Dictionary<string, IDatabase> _transientSchemas = new Dictionary<string, IDatabase>();

        public TransitionSchemaProvider(IDatabaseProvider schema)
        {
            _schemaProvider = schema;
        }

        public IDatabase GetDatabase(string database)
        {
            if (database != null && _transientSchemas.ContainsKey(database))
                return _transientSchemas[database];

            return _schemaProvider.GetDatabase(database);
        }

        public void AddTransitionSchema(IDatabase schema)
        {
            _transientSchemas.Add(schema.Name, schema);
        }
    }
}