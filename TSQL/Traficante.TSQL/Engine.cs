using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Traficante.TSQL.Converter;
using Traficante.TSQL.Evaluator.Tables;
using Traficante.TSQL.Lib;
using Traficante.TSQL.Schema;
using Traficante.TSQL.Schema.DataSources;
using Traficante.TSQL.Schema.Managers;

namespace Traficante.TSQL
{
    public class Engine : IEngine
    {
        private List<Database> _databases;
        private Library _library;
        public List<DatabaseVariable> _variables { get; private set; }

        public string DefaultDatabase = "master";
        public string DefaultSchema = "dbo";        

        public Engine()
        {
            _databases = new List<Database>();
            GetDatabaseOrCreate(DefaultDatabase);
            _variables = new List<DatabaseVariable>();
        }

        public Engine(Library library)
        {
            _databases = new List<Database>();
            _variables = new List<DatabaseVariable>();
            _library = library;
        }

        public Table Run(string script)
        {
            return new Runner().RunAndReturnTable(script, this);
            //var query = InstanceCreator.CompileForExecution(script, this);
            //return query.Run();
        }

        public void AddTable<T>(string table, IEnumerable<T> items)
        {
            AddTable(null, null, table, items);
        }

        public void AddTable<T>(string database, string schema, string table, IEnumerable<T> items)
        {
            database = database ?? DefaultDatabase;
            schema = schema ?? DefaultSchema;
            var db = GetDatabaseOrCreate(database);
            db.AddTable(schema, table, items);
        }

        public void AddFunction<T>(string database, string schema, string name, Func<IEnumerable<T>> function)
        {
            database = database ?? DefaultDatabase;
            schema = schema ?? DefaultSchema;
            var db = GetDatabaseOrCreate(database);
            db.AddFunction(schema, name, function);
        }

        public void AddFunction<TResult>(string database, string schema, string name, Func<TResult> function)
        {
            database = database ?? DefaultDatabase;
            schema = schema ?? DefaultSchema;
            var db = GetDatabaseOrCreate(database);
            db.AddFunction(schema, name, function);
        }

        public void AddFunction<T1, TResult>(string database, string schema, string name, Func<T1, TResult> function)
        {
            database = database ?? DefaultDatabase;
            schema = schema ?? DefaultSchema;
            var db = GetDatabaseOrCreate(database);
            db.AddFunction(schema, name, function);
        }

        public void AddFunction<T1, T2, TResult>(string database, string schema, string name, Func<T1, T2, TResult> function)
        {
            database = database ?? DefaultDatabase;
            schema = schema ?? DefaultSchema;
            var db = GetDatabaseOrCreate(database);
            db.AddFunction(schema, name, function);
        }

        public void AddFunction<T1, T2, T3, TResult>(string database, string schema, string name, Func<T1, T2, T3, TResult> function)
        {
            database = database ?? DefaultDatabase;
            schema = schema ?? DefaultSchema;
            var db = GetDatabaseOrCreate(database);
            db.AddFunction(schema, name, function);
        }

        public void AddFunction<T1, T2, T3, T4, TResult>(string database, string schema, string name, Func<T1, T2, T3, T4, TResult> function)
        {
            database = database ?? DefaultDatabase;
            schema = schema ?? DefaultSchema;
            var db = GetDatabaseOrCreate(database);
            db.AddFunction(schema, name, function);
        }

        public void AddFunction<T1, T2, T3, T4, T5, TResult>(string database, string schema, string name, Func<T1, T2, T3, T4, T5, TResult> function)
        {
            database = database ?? DefaultDatabase;
            schema = schema ?? DefaultSchema;
            var db = GetDatabaseOrCreate(database);
            db.AddFunction(schema, name, function);
        }


        public void SetVariable<T>(string name, T value)
        {
            SetVariable(null, null, name, value);
        }

        public void SetVariable<T>(string database, string schema, string name, T value)
        {
            var variable = _variables.FirstOrDefault(x => string.Equals(x.Schema, schema, StringComparison.CurrentCultureIgnoreCase) && string.Equals(x.Name, name, StringComparison.CurrentCultureIgnoreCase));
            if (variable != null)
            {
                variable.Value = value;
            }
            else
            {
                _variables.Add(new DatabaseVariable(schema, name, typeof(T), value));
            }
        }

        public void SetVariable(string name, Type type, object value)
        {
            SetVariable(null, null, name, type, value);
        }

        public void SetVariable(string database, string schema, string name, Type type, object value)
        {
            var variable = _variables.FirstOrDefault(x => string.Equals(x.Schema, schema, StringComparison.CurrentCultureIgnoreCase) && string.Equals(x.Name, name, StringComparison.CurrentCultureIgnoreCase));
            if (variable != null)
            {
                variable.Value = value;
            }
            else
            {
                _variables.Add(new DatabaseVariable(schema, name, type, value));
            }
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
        [ThreadStaticAttribute]
        public Engine eng;
        public Database GetDatabaseOrCreate(string database)
        {
            var db = _databases.FirstOrDefault(x => string.Equals(x.Name, database, StringComparison.CurrentCultureIgnoreCase));
            if (db == null)
            {
                db = new Database(database, DefaultSchema, this);//, _library);
                eng = this;
                db.AddFunction<string, IEnumerable<object>>(null, "exec", (x) =>
                {
                    var that = this;
                    return new List<object>();
                });

                _databases.Add(db);
            }
            return db;
        }
    }
}
