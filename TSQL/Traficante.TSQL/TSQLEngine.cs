using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Traficante.TSQL.Converter;
using Traficante.TSQL.Evaluator.Visitors;
using Traficante.TSQL.Lib;
using Traficante.TSQL.Schema.DataSources;
using Traficante.TSQL.Schema.Managers;
using MethodInfo = Traficante.TSQL.Schema.Managers.MethodInfo;

namespace Traficante.TSQL
{
    public class TSQLEngine : IDisposable
    {
        public SchemaManager SchemaManager { get; set; } = new SchemaManager();
        public VariableManager VariableManager { get; set; } = new VariableManager();

        public DataManager DataManager { get; set; }

        public TSQLEngine()
        {
            DataManager = new DataManager(this);
            SchemaManager.RegisterLibraries(new Library());
        }

        private Runner runner;
        public IEnumerable Run(string script, CancellationToken ct = default)
        {
            runner = new Runner();
            return runner.Run(script, this, ct) as IEnumerable;
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
            SchemaManager.RegisterTable(new TableInfo(name, path ?? new string[0], items));
        }

        public void AddTable<TResult>(string name, string[] path, Func<TResult> function)
        {
            SchemaManager.RegisterTable(
                new TableInfo(name, path ?? new string[0],
                    new MethodInfo
                    {
                        FunctionDelegate = function,
                        FunctionMethod = function?.Method,
                    }));
        }

        public void AddFunction<TResult>(string name, string[] path, Func<TResult> function)
        {
            this.SchemaManager.RegisterMethod(name, path, function);
        }

        public void AddFunction<TResult>(string name, Func<TResult> function)
        {
            this.SchemaManager.RegisterMethod(name, function);
        }

        public void AddFunction<T1, TResult>(string name, string[] path, Func<T1, TResult> function)
        {
            this.SchemaManager.RegisterMethod(name, path, function);
        }

        public void AddFunction<T1, TResult>(string name, Func<T1, TResult> function)
        {
            this.SchemaManager.RegisterMethod(name, function);
        }

        public void AddFunction<T1, T2, TResult>(string name, string[] path, Func<T1, T2, TResult> function)
        {
            this.SchemaManager.RegisterMethod(name, path, function);
        }

        public void AddFunction<T1, T2, TResult>(string name, Func<T1, T2, TResult> function)
        {
            this.SchemaManager.RegisterMethod(name, function);
        }

        public void AddFunction<T1, T2, T3, TResult>(string name, string[] path, Func<T1, T2, T3, TResult> function)
        {
            this.SchemaManager.RegisterMethod(name, path, function);
        }

        public void AddFunction<T1, T2, T3, TResult>(string name, Func<T1, T2, T3, TResult> function)
        {
            this.SchemaManager.RegisterMethod(name, function);
        }

        public void AddFunction<T1, T2, T3, T4, TResult>(string name, string[] path, Func<T1, T2, T3, T4, TResult> function)
        {
            this.SchemaManager.RegisterMethod(name, path, function);
        }

        public void AddFunction<T1, T2, T3, T4, TResult>(string name, Func<T1, T2, T3, T4, TResult> function)
        {
            this.SchemaManager.RegisterMethod(name, function);
        }

        public void AddFunction<T1, T2, T3, T4, T5, TResult>(string name, string[] path, Func<T1, T2, T3, T4, T5, TResult> function)
        {
            this.SchemaManager.RegisterMethod(name, path, function);
        }

        public void AddFunction<T1, T2, T3, T4, T5, TResult>(string name, Func<T1, T2, T3, T4, T5, TResult> function)
        {
            this.SchemaManager.RegisterMethod(name, function);
        }

        public void SetVariable<T>(string name, T value)
        {
            this.VariableManager.SetVariable<T>(name, value);
        }

        public void SetVariable(string name, Type type, object value)
        {
            this.VariableManager.SetVariable(name, type, value);
        }

        public Variable GetVariable(string name)
        {
            return this.VariableManager.GetVariable(name);
        }

        public MethodInfo ResolveMethod(string method, string[] path, Type[] parameters)
        {
            return SchemaManager.ResolveMethod(method, path, parameters);
        }

        public TableInfo ResolveTable(string name, string[] path)
        {
            return SchemaManager.ResolveTable(name, path);
        }

        public void AddMethodResolver(Func<string, string[], Type[], Delegate> resolver)
        {
            SchemaManager.RegisterMethodResolver(resolver);
        }

        public void AddTableResolver(Func<string, string[], Delegate> resolver)
        {
            SchemaManager.RegisterTableResolver(resolver);
        }

        public void Dispose()
        {
        }
    }



}
