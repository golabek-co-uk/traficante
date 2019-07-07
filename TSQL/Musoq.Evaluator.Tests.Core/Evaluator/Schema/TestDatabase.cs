using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Musoq.Schema;
using Musoq.Schema.DataSources;
using Musoq.Schema.Helpers;
using Musoq.Schema.Managers;
using Musoq.Schema.Reflection;

namespace Musoq.Evaluator.Tests.Core.Schema
{
    public class TestDatabase<T> : Database
        //where T : BasicEntity
    {
        private readonly IDictionary<string, IEnumerable<T>> _sources;

        public TestDatabase(IDictionary<string, IEnumerable<T>> sources)
            : base("test", CreateLibrary())
        {
            _sources = sources;
            foreach(var source in sources)
            {
                //AddSource<EntitySource<T>>(source.Key, "entities", source.Value);
                //AddTable<BasicEntityTable>(source.Key, "entities");
                AddTable(source.Key, "entities", source.Value);
                AddFunction(source.Key, "Entities", () => source.Value);
            }
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
    }
}