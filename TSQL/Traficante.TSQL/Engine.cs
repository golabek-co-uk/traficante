using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Traficante.TSQL.Converter;
using Traficante.TSQL.Evaluator.Tables;
using Traficante.TSQL.Plugins;
using Traficante.TSQL.Schema;
using Traficante.TSQL.Schema.DataSources;
using Traficante.TSQL.Schema.Managers;

namespace Traficante.TSQL
{
    public class Engine : IEngine
    {
        private List<Database> _databases;
        private LibraryBase _library;
        public List<DatabaseVariable> _variables { get; private set; }

        public string DefaultDatabase = "master";
        public string DefaultSchema = "dbo";        

        public Engine()
        {
            _databases = new List<Database>();
            _databases.Add(new Database(DefaultDatabase, DefaultSchema));
            _variables = new List<DatabaseVariable>();
        }

        public Engine(LibraryBase library)
        {
            _databases = new List<Database>();
            _variables = new List<DatabaseVariable>();
            _library = library;
        }

        public Table Run(string script)
        {
            var query = InstanceCreator.CompileForExecution(script, this);
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
                db = new Database(database, DefaultSchema, _library);
                _databases.Add(db);
            }
            db.AddTable(schema, table, items);
        }

        public void AddFunction<T>(string database, string schema, string table, Func<IEnumerable<T>> function)
        {
            database = database ?? DefaultDatabase;
            schema = schema ?? DefaultSchema;
            var db = _databases.FirstOrDefault(x => string.Equals(x.Name, database, StringComparison.CurrentCultureIgnoreCase));
            if (db == null)
            {
                db = new Database(database, DefaultSchema, _library);
                _databases.Add(db);
            }
            db.AddFunction(schema, table, function);
        }

        public void AddVariable<T>(string name, T value)
        {
            AddVariable(null, null, name, value);
        }

        public void AddVariable<T>(string database, string schema, string name, T value)
        {
            _variables.Add(new DatabaseVariable(schema, name, typeof(T), value));
        }

        public IVariable GetVariable(string name)
        {
            return _variables.FirstOrDefault(x => string.Equals(x.Name, name, StringComparison.CurrentCultureIgnoreCase));
        }

        public IDatabase GetDatabase(string database)
        {
            if (database != null)
                return _databases.FirstOrDefault(x => string.Equals(x.Name, database, System.StringComparison.CurrentCultureIgnoreCase));
            else
                return _databases.FirstOrDefault(x => string.Equals(x.Name, DefaultDatabase));
        }
    }
}
