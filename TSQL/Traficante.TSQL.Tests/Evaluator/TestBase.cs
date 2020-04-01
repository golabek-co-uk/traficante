using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Traficante.TSQL.Converter;
using Traficante.TSQL.Evaluator.Tests.Core.Schema;
using Traficante.TSQL.Lib;
using Traficante.TSQL.Schema;
using Traficante.TSQL.Schema.Managers;
using Environment = System.Environment;
using Traficante.TSQL.Tests;

namespace Traficante.TSQL.Evaluator.Tests.Core
{
    public class CompiledQuery
    {
        private DataTable _table;
        public CompiledQuery(DataTable table)
        {
            _table = table;
        }

        public DataTable Run()
        {
            return _table;
        }

        public DataTable Run(CancellationToken token)
        {
            return _table;
        }
    }

    public class TestBase
    {
        protected CancellationTokenSource TokenSource { get; } = new CancellationTokenSource();
        Random _random = new Random();

        protected CompiledQuery CreateAndRunVirtualMachine<T>(string script, IDictionary<string, IEnumerable<T>> sources)
        {
            var engine = new TSQLEngine();
            //new TestLibrary()
            engine.AddFunction<int?, int?>("NullableMethod", null, x => x);
            engine.AddFunction<int>("RandomNumber", () => _random.Next(0, 100));
            engine.AddFunction<decimal>("GetOne", () => 1);
            engine.AddFunction<decimal, string,string>("GetTwo", (a,b) => 2.ToString());
            engine.AddFunction<decimal, decimal>("Inc", (number) => number + 1);
            engine.AddFunction<long, long>("Inc", (number) => number + 1);
            engine.AddFunction<BasicEntity, BasicEntity>("NothingToDo", (entity) => entity);
            engine.AddFunction<int?, int?>("NullableMethod", (value) => value);
            engine.AddFunction<object, string>("ToString", (obj) => obj.ToString());
            engine.AddFunction<long, decimal, bool, bool, string, int>("PrimitiveArgumentsMethod", (a, b, tr, fl, text) =>
            {
                Assert.AreEqual(1L, a);
                Assert.AreEqual(2m, b);
                Assert.AreEqual(true, tr);
                Assert.AreEqual(false, fl);
                Assert.AreEqual("text", text);
                return 1;
            });

            foreach (var source in sources)
            {
                engine.AddTable("entities", new string[1] { source.Key }, source.Value);
                engine.AddFunction("Entities", new string[1] { source.Key }, () => source.Value);
            }
            return new CompiledQuery(engine.RunAndReturnTable(script));
            //return InstanceCreator.CompileForExecution(script, engine);
        }

        protected void TestMethodTemplate<TResult>(string operation, TResult score)
        {
            var query = $"select {operation} from #A.Entities()";
            var sources = new Dictionary<string, IEnumerable<BasicEntity>>
            {
                {"#A", new[] {new BasicEntity("ABCAACBA")}}
            };

            var vm = CreateAndRunVirtualMachine(query, sources);
            var table = vm.Run();

            Assert.AreEqual(1, table.Count);
            Assert.AreEqual(typeof(TResult), table.Columns.ElementAt(0).ColumnType);

            Assert.AreEqual(score, table[0][0]);
        }

        static TestBase()
        {
            new Lib.Environment().SetValue(Constants.NetStandardDllEnvironmentName, EnvironmentUtils.GetOrCreateEnvironmentVariable());
        }
    }
}