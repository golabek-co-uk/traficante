using System.Collections.Generic;
using System.Linq;

namespace Traficante.TSQL.Schema
{
    public class SchemaProvider : ISchemaProvider
    {
        private IEnumerable<ISchema> _schemas;

        public SchemaProvider(IEnumerable<ISchema> schemas)
        {
            this._schemas = schemas;
        }

        public ISchema GetDatabase(string database)
        {
            if (database != null)
                return _schemas.FirstOrDefault(x => x.Name == database);
            return _schemas.FirstOrDefault();
        }
    }
}