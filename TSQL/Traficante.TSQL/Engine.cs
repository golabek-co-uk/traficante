using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
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
using MethodInfo = Traficante.TSQL.Schema.Managers.MethodInfo;

namespace Traficante.TSQL
{
    public class Engine : IDisposable
    {
        public SourceDataManager SourceDataManager { get; set; } = new SourceDataManager();
        public List<Variable> Variables { get; private set; }
        internal List<IDisposable> Disposables { get; private set; } = new List<IDisposable>();
        public Engine()
        {
            Variables = new List<Variable>();
            SourceDataManager.RegisterLibraries(new Library());
        }

        public Evaluator.Tables.DataTable Run(string script)
        {
            return new Runner().RunAndReturnTable(script, this);
        }

        public void AddTable<T>(string name, IEnumerable<T> items)
        {
            AddTable(name, new string[0],  items);
        }

        public void AddTable<TResult>(string name, Func<TResult> function)
        {
            AddTable(name, new string[0], function);
        }

        public void AddTable<T>(string name, string[] path, IEnumerable<T> items)
        {
            SourceDataManager.RegisterTable(new TableInfo(name, path ?? new string[0], items));
        }

        public void AddTable<TResult>(string name, string[] path, Func<TResult> function)
        {
            SourceDataManager.RegisterTable(
                new TableInfo(name, path ?? new string[0],
                    new MethodInfo
                    {
                        FunctionDelegate = function,
                        FunctionMethod = function?.Method,
                    }));
        }

        public void AddFunction<TResult>(string name, string[] path, Func<TResult> function)
        {
            this.SourceDataManager.RegisterMethod(name, path, function);
        }

        public void AddFunction<TResult>(string name, Func<TResult> function)
        {
            this.SourceDataManager.RegisterMethod(name, function);
        }

        public void AddFunction<T1, TResult>(string name, string[] path, Func<T1, TResult> function)
        {
            this.SourceDataManager.RegisterMethod(name, path, function);
        }

        public void AddFunction<T1, TResult>(string name, Func<T1, TResult> function)
        {
            this.SourceDataManager.RegisterMethod(name, function);
        }

        public void AddFunction<T1, T2, TResult>(string name, string[] path, Func<T1, T2, TResult> function)
        {
            this.SourceDataManager.RegisterMethod(name, path, function);
        }

        public void AddFunction<T1, T2, TResult>(string name, Func<T1, T2, TResult> function)
        {
            this.SourceDataManager.RegisterMethod(name, function);
        }

        public void AddFunction<T1, T2, T3, TResult>(string name, string[] path, Func<T1, T2, T3, TResult> function)
        {
            this.SourceDataManager.RegisterMethod(name, path, function);
        }

        public void AddFunction<T1, T2, T3, TResult>(string name, Func<T1, T2, T3, TResult> function)
        {
            this.SourceDataManager.RegisterMethod(name, function);
        }

        public void AddFunction<T1, T2, T3, T4, TResult>(string name, string[] path, Func<T1, T2, T3, T4, TResult> function)
        {
            this.SourceDataManager.RegisterMethod(name, path, function);
        }

        public void AddFunction<T1, T2, T3, T4, TResult>(string name, Func<T1, T2, T3, T4, TResult> function)
        {
            this.SourceDataManager.RegisterMethod(name, function);
        }

        public void AddFunction<T1, T2, T3, T4, T5, TResult>(string name, string[] path, Func<T1, T2, T3, T4, T5, TResult> function)
        {
            this.SourceDataManager.RegisterMethod(name, path, function);
        }

        public void AddFunction<T1, T2, T3, T4, T5, TResult>(string name, Func<T1, T2, T3, T4, T5, TResult> function)
        {
            this.SourceDataManager.RegisterMethod(name, function);
        }

        public void SetVariable<T>(string name, T value)
        {
            var variable = Variables.FirstOrDefault(x => string.Equals(x.Name, name, StringComparison.CurrentCultureIgnoreCase));
            if (variable != null)
            {
                variable.Value = value;
            }
            else
            {
                Variables.Add(new Variable(name, typeof(T), value));
            }
        }

        public void SetVariable(string name, Type type, object value)
        {
            var variable = Variables.FirstOrDefault(x => string.Equals(x.Name, name, StringComparison.CurrentCultureIgnoreCase));
            if (variable != null)
            {
                variable.Value = value;
            }
            else
            {
                Variables.Add(new Variable(name, type, value));
            }
        }

        public Variable GetVariable(string name)
        {
            return Variables.FirstOrDefault(x => string.Equals(x.Name, name, StringComparison.CurrentCultureIgnoreCase));
        }

        public MethodInfo ResolveMethod(string method, string[] path, Type[] parameters)
        {
            return SourceDataManager.ResolveMethod(method, path, parameters);
        }

        public TableInfo ResolveTable(string name, string[] path)
        {
            return SourceDataManager.ResolveTable(name, path);
        }

        public void AddMethodResolver(Func<string, string[], Type[], Delegate> resolver)
        {
            SourceDataManager.RegisterMethodResolver(resolver);
        }

        public void AddTableResolver(Func<string, string[], Delegate> resolver)
        {
            SourceDataManager.RegisterTableResolver(resolver);
        }

        public void Dispose()
        {
            foreach (var disposable in Disposables)
                disposable.Dispose();
        }
    }
}
