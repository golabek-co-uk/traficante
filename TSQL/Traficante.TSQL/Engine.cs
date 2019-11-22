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
        public List<TableInfo> Tables { get; set; } = new List<TableInfo>();
        public MethodsManager MethodsManager { get; set; } = new MethodsManager();

        public List<Variable> _variables { get; private set; }
        public Engine()
        {
            _variables = new List<Variable>();
            MethodsManager.RegisterLibraries(new Library());
        }

        public Evaluator.Tables.Table Run(string script)
        {
            return new Runner().RunAndReturnTable(script, this);
        }

        public void AddTable<T>(string name, IEnumerable<T> items)
        {
            AddTable(name, new string[0],  items);
        }

        public void AddTable<T>(string name, string[] path, IEnumerable<T> items)
        {
            Tables.Add(new TableInfo(name, path ?? new string[0], items, typeof(T)));
        }

        public TableInfo ResolveTable(string name, string[] path)
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

        public void AddFunction<T>(string name, string[] path, Func<IEnumerable<T>> function)
        {
            this.MethodsManager.RegisterMethod(name, path, function);
        }

        public void AddFunction<T>(string name, Func<IEnumerable<T>> function)
        {
            this.MethodsManager.RegisterMethod(name, function);
        }

        public void AddFunction<TResult>(string name, string[] path, Func<TResult> function)
        {
            this.MethodsManager.RegisterMethod(name, path, function);
        }

        public void AddFunction<TResult>(string name, Func<TResult> function)
        {
            this.MethodsManager.RegisterMethod(name, function);
        }

        public void AddFunction<T1, TResult>(string name, string[] path, Func<T1, TResult> function)
        {
            this.MethodsManager.RegisterMethod(name, path, function);
        }

        public void AddFunction<T1, TResult>(string name, Func<T1, TResult> function)
        {
            this.MethodsManager.RegisterMethod(name, function);
        }

        public void AddFunction<T1, T2, TResult>(string name, string[] path, Func<T1, T2, TResult> function)
        {
            this.MethodsManager.RegisterMethod(name, path, function);
        }

        public void AddFunction<T1, T2, TResult>(string name, Func<T1, T2, TResult> function)
        {
            this.MethodsManager.RegisterMethod(name, function);
        }

        public void AddFunction<T1, T2, T3, TResult>(string name, string[] path, Func<T1, T2, T3, TResult> function)
        {
            this.MethodsManager.RegisterMethod(name, path, function);
        }

        public void AddFunction<T1, T2, T3, TResult>(string name, Func<T1, T2, T3, TResult> function)
        {
            this.MethodsManager.RegisterMethod(name, function);
        }

        public void AddFunction<T1, T2, T3, T4, TResult>(string name, string[] path, Func<T1, T2, T3, T4, TResult> function)
        {
            this.MethodsManager.RegisterMethod(name, path, function);
        }

        public void AddFunction<T1, T2, T3, T4, TResult>(string name, Func<T1, T2, T3, T4, TResult> function)
        {
            this.MethodsManager.RegisterMethod(name, function);
        }

        public void AddFunction<T1, T2, T3, T4, T5, TResult>(string name, string[] path, Func<T1, T2, T3, T4, T5, TResult> function)
        {
            this.MethodsManager.RegisterMethod(name, path, function);
        }

        public void AddFunction<T1, T2, T3, T4, T5, TResult>(string name, Func<T1, T2, T3, T4, T5, TResult> function)
        {
            this.MethodsManager.RegisterMethod(name, function);
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

        public (MethodInfo MethodInfo, Delegate Delegate) ResolveMethod(string[] path, string method, Type[] parameters)
        {
            return MethodsManager.GetMethod(method, path, parameters);
        }
    }

    public class TableInfo
    {
        public TableInfo()
        {
        }
        public TableInfo(string name, string[] path, IEnumerable items, Type itemsType)
        {
            this.Name = name;
            this.Path = path;
            this.Items = items;
            this.ItemsType = itemsType;
        }
        public string Name { get; set; }
        public string[] Path { get; set; }
        public IEnumerable Items { get; set; }
        public Type ItemsType { get; set; }
    }
}
