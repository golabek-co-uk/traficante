using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Traficante.TSQL.Converter;
using Traficante.TSQL.Evaluator.Tables;
using Traficante.TSQL.Lib;
using Traficante.TSQL.Schema;
using Traficante.TSQL.Schema.DataSources;
using Traficante.TSQL.Schema.Helpers;
using Traficante.TSQL.Schema.Managers;

namespace Traficante.TSQL
{
    public class Engine : IEngine
    {
        public List<(string Name, string[] Path, IEnumerable Items, Type ItemsType)> Tables { get; set; } = new List<(string Name, string[] Path, IEnumerable Items, Type ItemsType)>();
        public List<(string Name, string[] Path, IEnumerable Items, Type ItemsType)> Functions { get; set; } = new List<(string Name, string[] Path, IEnumerable Items, Type ItemsType)>();
        public MethodsManager MethodsManager { get; set; } = new MethodsManager();

        public List<Variable> _variables { get; private set; }

        public string DefaultDatabase = "master";
        public string DefaultSchema = "dbo";        

        public Engine()
        {
            _variables = new List<Variable>();
            MethodsManager.RegisterLibraries(new Library());
        }

        public Table Run(string script)
        {
            return new Runner().RunAndReturnTable(script, this);
        }

        public void AddTable<T>(string table, IEnumerable<T> items)
        {
            AddTable(null, null, table, items);
        }

        public void AddTable<T>(string database, string schema, string name, IEnumerable<T> items)
        {
            database = database ?? DefaultDatabase;
            schema = schema ?? DefaultSchema;
            Tables.Add((name, new string[2] { database, schema }, items, typeof(T)));
        }

        public (string Name, string[] Path, IEnumerable Items, Type ItemsType) GetTable(string name, string[] path)
        {
            return Tables
                .Where(x => string.Equals(x.Name, name, StringComparison.InvariantCultureIgnoreCase))
                .Where(x =>
                {
                    var pathOfX = x.Path.Reverse().ToList();
                    var pathToFind = path.Reverse().ToList();
                    for (int i = 0; i < pathToFind.Count; i++)
                    {
                        if (pathOfX.ElementAtOrDefault(i) != pathToFind.ElementAtOrDefault(i))
                            return false;
                    }
                    return true;
                }).FirstOrDefault();
        }

        public (string Name, string[] Path, IEnumerable Items, Type ItemsType) GetFunction(string name, string[] path)
        {
            return Functions
                .Where(x => string.Equals(x.Name, name, StringComparison.InvariantCultureIgnoreCase))
                .Where(x =>
                {
                    var pathOfX = x.Path.Reverse().ToList();
                    var pathToFind = path.Reverse().ToList();
                    for (int i = 0; i < pathToFind.Count; i++)
                    {
                        if (pathOfX.ElementAtOrDefault(i) != pathToFind.ElementAtOrDefault(i))
                            return false;
                    }
                    return true;
                }).FirstOrDefault();
        }

        public void AddFunction<T>(string database, string schema, string name, Func<IEnumerable<T>> function)
        {
            database = database ?? DefaultDatabase;
            schema = schema ?? DefaultSchema;
            this.MethodsManager.RegisterMethod(name, function.Method);
            Functions.Add((name, new string[2] { database, schema }, function(), typeof(T)));
        }

        public void AddFunction<TResult>(string database, string schema, string name, Func<TResult> function)
        {
            this.MethodsManager.RegisterMethod(name, function.Method);
        }

        public void AddFunction<T1, TResult>(string database, string schema, string name, Func<T1, TResult> function)
        {
            this.MethodsManager.RegisterMethod(name, function.Method);
        }

        public void AddFunction<T1, T2, TResult>(string database, string schema, string name, Func<T1, T2, TResult> function)
        {
            this.MethodsManager.RegisterMethod(name, function.Method);
        }

        public void AddFunction<T1, T2, T3, TResult>(string database, string schema, string name, Func<T1, T2, T3, TResult> function)
        {
            this.MethodsManager.RegisterMethod(name, function.Method);
        }

        public void AddFunction<T1, T2, T3, T4, TResult>(string database, string schema, string name, Func<T1, T2, T3, T4, TResult> function)
        {
            this.MethodsManager.RegisterMethod(name, function.Method);
        }

        public void AddFunction<T1, T2, T3, T4, T5, TResult>(string database, string schema, string name, Func<T1, T2, T3, T4, T5, TResult> function)
        {
            this.MethodsManager.RegisterMethod(name, function.Method);
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
                _variables.Add(new Variable(schema, name, typeof(T), value));
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
                _variables.Add(new Variable(schema, name, type, value));
            }
        }

        public IVariable GetVariable(string name)
        {
            return _variables.FirstOrDefault(x => string.Equals(x.Name, name, StringComparison.CurrentCultureIgnoreCase));
        }

        public bool TryResolveAggreationMethod(string method, Type[] parameters, out MethodInfo methodInfo)
        {
            return MethodsManager.TryGetMethod(method, parameters, out methodInfo);
        }

        public MethodInfo ResolveMethod(string[] path, string method, Type[] parameters)
        {
            var schema = path.Reverse().ElementAtOrDefault(0);
            return MethodsManager.GetMethod(method, parameters);
        }
    }

}
