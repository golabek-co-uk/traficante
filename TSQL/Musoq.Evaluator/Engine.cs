using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Traficante.TSQL.Converter;
using Traficante.TSQL.Evaluator.Tables;
using Traficante.TSQL.Schema;

namespace Traficante.TSQL
{
    public class Engine
    {
        private List<Database> _databases;

        public string DefaultDatabase = "master";
        public string DefaultSchema = "dbo";

        public Engine()
        {
            _databases = new List<Database>();
        }

        public Table Run(string script)
        {
            var query = InstanceCreator.CompileForExecution(script, new DatabaseProvider(_databases.ToArray(), DefaultDatabase));
            return query.Run();
        }

        public void AddTable<T>(string table, IEnumerable<T> items)
        {
            AddTable(null, null, table, items);
        }

        public void AddTable<T>(string database, string schema, string table, IEnumerable<T> items)
        {
            database = database ?? DefaultDatabase;
            schema = schema ?? DefaultSchema;
            var db = _databases.FirstOrDefault(x => string.Equals(x.Name, database, StringComparison.CurrentCultureIgnoreCase));
            if (db == null)
            {
                db = new Database(database, DefaultSchema);
                _databases.Add(db);
            }
            db.AddTable(schema, table, items);
        }

        public void AddVariable<T>(string name, T value)
        {
            AddVariable(null, null, name, value);
        }

        public void AddVariable<T>(string database, string schema, string name, T value)
        {
            database = database ?? DefaultDatabase;
            schema = schema ?? DefaultSchema;
            var db = _databases.FirstOrDefault(x => string.Equals(x.Name, database, StringComparison.CurrentCultureIgnoreCase));
            if (db == null)
            {
                db = new Database(database, DefaultSchema);
                _databases.Add(db);
            }
            db.AddVariable(schema, name, value);
        }
    }
}
