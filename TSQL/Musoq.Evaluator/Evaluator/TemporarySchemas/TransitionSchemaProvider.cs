using System.Collections.Generic;
using Traficante.TSQL.Schema;

namespace Traficante.TSQL.Evaluator.TemporarySchemas
{
    public class TransitionSchemaProvider : ISchemaProvider
    {
        private readonly ISchemaProvider _schemaProvider;
        private readonly Dictionary<string, ISchema> _transientSchemas = new Dictionary<string, ISchema>();

        public TransitionSchemaProvider(ISchemaProvider schema)
        {
            _schemaProvider = schema;
        }

        public ISchema GetDatabase(string database)
        {
            if (database != null && _transientSchemas.ContainsKey(database))
                return _transientSchemas[database];

            return _schemaProvider.GetDatabase(database);
        }

        public void AddTransitionSchema(ISchema schema)
        {
            _transientSchemas.Add(schema.Name, schema);
        }
    }
}