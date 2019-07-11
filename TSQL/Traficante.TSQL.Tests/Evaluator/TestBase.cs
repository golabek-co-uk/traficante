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
using Traficante.TSQL.Plugins;
using Traficante.TSQL.Schema;
using Traficante.TSQL.Schema.Managers;
using Environment = System.Environment;

namespace Traficante.TSQL.Evaluator.Tests.Core
{
    public class TestBase
    {
        protected CancellationTokenSource TokenSource { get; } = new CancellationTokenSource();

        protected CompiledQuery CreateAndRunVirtualMachine<T>(string script, IDictionary<string, IEnumerable<T>> sources)
        {
            var engine = new Engine(new TestLibrary());

            foreach (var source in sources)
            {
                engine.AddTable(null, source.Key, "entities", source.Value);
                engine.AddFunction(null, source.Key, "Entities", () => source.Value);
            }

            return InstanceCreator.CompileForExecution(script, engine);
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

        private static MethodsAggregator CreateLibrary()
        {
            var methodManager = new MethodsManager();
            var propertiesManager = new PropertiesManager();

            var lib = new TestLibrary();

            propertiesManager.RegisterProperties(lib);
            methodManager.RegisterLibraries(lib);

            return new MethodsAggregator(methodManager, propertiesManager);
        }

        static TestBase()
        {
            new Plugins.Environment().SetValue(Constants.NetStandardDllEnvironmentName, EnvironmentUtils.GetOrCreateEnvironmentVariable());
        }
    }
}