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
    public class TestDatabase<T> : DatabaseBase
        //where T : BasicEntity
    {
        private static readonly IDictionary<string, int> TestNameToIndexMap;
        private static readonly IDictionary<int, Func<T, object>> TestIndexToObjectAccessMap;
        private readonly IDictionary<string, IEnumerable<T>> _sources;

        static TestDatabase()
        {
            TestNameToIndexMap = new Dictionary<string, int>
            {
                {nameof(BasicEntity.Name), 10},
                {nameof(BasicEntity.City), 11},
                {nameof(BasicEntity.Country), 12},
                {nameof(BasicEntity.Population), 13},
                {nameof(BasicEntity.Self), 14},
                {nameof(BasicEntity.Money), 15},
                {nameof(BasicEntity.Month), 16},
                {nameof(BasicEntity.Time), 17},
                {nameof(BasicEntity.Id), 18},
                {nameof(BasicEntity.NullableValue), 19}
            };

            TestIndexToObjectAccessMap = new Dictionary<int, Func<T, object>>
            {
                {10, arg => (arg as BasicEntity).Name},
                {11, arg => (arg as BasicEntity).City},
                {12, arg => (arg as BasicEntity).Country},
                {13, arg => (arg as BasicEntity).Population},
                {14, arg => (arg as BasicEntity).Self},
                {15, arg => (arg as BasicEntity).Money},
                {16, arg => (arg as BasicEntity).Month},
                {17, arg => (arg as BasicEntity).Time},
                {18, arg => (arg as BasicEntity).Id},
                {19, arg => (arg as BasicEntity).NullableValue},
            };
        }

        public TestDatabase(IDictionary<string, IEnumerable<T>> sources)
            : base("test", CreateLibrary())
        {
            _sources = sources;
            foreach(var source in sources)
            {
                AddSource<EntitySource<T>>(source.Key, "entities", source.Value, TestNameToIndexMap, TestIndexToObjectAccessMap);
                AddTable<BasicEntityTable>(source.Key, "entities");
            }
            //AddSource<EntitySource<T>>("entities", _sources.Keys.First(), _sources.Values.First(), TestNameToIndexMap, TestIndexToObjectAccessMap);
            //AddTable<BasicEntityTable>("entities", _sources.Keys.First());
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