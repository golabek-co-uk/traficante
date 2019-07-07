using System;
using System.Collections.Generic;
using System.Text;
using Traficante.TSQL.Converter;
using Traficante.TSQL.Evaluator.Tables;
using Traficante.TSQL.Schema;

namespace Traficante.TSQL
{
    public class Engine
    {
        private ServerSchema _schema;

        public Engine()
        {
            _schema = new ServerSchema();
        }

        public Table Run(string script)
        {
            var query = InstanceCreator.CompileForExecution(script, new SchemaProvider(new[] { _schema }));
            return query.Run();
        }

        public void AddTable<T>(string table, IEnumerable<T> items)
        {
            AddTable(null, null, table, items);
        }

        public void AddTable<T>(string database, string schema, string table, IEnumerable<T> items)
        {
            _schema.AddTable(schema, table, items);
        }
    }
}
