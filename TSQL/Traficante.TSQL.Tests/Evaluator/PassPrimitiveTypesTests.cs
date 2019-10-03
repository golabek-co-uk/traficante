using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Traficante.TSQL.Converter;
using Traficante.TSQL.Evaluator.Tests.Core.Schema;
using Traficante.TSQL.Schema;
using Traficante.TSQL.Schema.DataSources;
using Traficante.TSQL.Schema.Managers;
using Traficante.TSQL.Schema.Reflection;

namespace Traficante.TSQL.Evaluator.Tests.Core
{
    [Ignore]
    [TestClass]
    public class PassPrimitiveTypesTests : TestBase
    {
        [TestMethod]
        public void GetSchemaTableAndRowSourcePassedPrimitiveArgumentsTest()
        {
            var query = "select 1 from #test.whatever(1, 2d, true, false, 'text')";

            var vm = CreateAndRunVirtualMachine(query, new List<TestEntity>(), (passedParams) =>
            {
                Assert.AreEqual(1, passedParams[0]);
                Assert.AreEqual(2m, passedParams[1]);
                Assert.AreEqual(true, passedParams[2]);
                Assert.AreEqual(false, passedParams[3]);
                Assert.AreEqual("text", passedParams[4]);
            }, WhenCheckedParameters.OnSchemaTableOrRowSourceGet);

            vm.Run();
        }

        [TestMethod]
        public void CallWithPrimitiveArgumentsTest()
        {
            var query = "select PrimitiveArgumentsMethod(1, 2d, true, false, 'text') from #test.whatever()";

            var vm = CreateAndRunVirtualMachine(query, new List<TestEntity>(), (passedParams) =>
            {
                Assert.AreEqual(1L, passedParams[0]);
                Assert.AreEqual(2m, passedParams[1]);
                Assert.AreEqual(true, passedParams[2]);
                Assert.AreEqual(false, passedParams[3]);
                Assert.AreEqual("text", passedParams[4]);
            }, WhenCheckedParameters.OnMethodCall);

            vm.Run();
        }

        private enum WhenCheckedParameters
        {
            OnSchemaTableOrRowSourceGet,
            OnMethodCall
        }

        private class TestSchemaProvider : IEngine
        {
            private readonly IEnumerable<TestEntity> _entities;
            private readonly Action<object[]> _onGetTableOrRowSource;
            private readonly WhenCheckedParameters _whenChecked;

            public TestSchemaProvider(IEnumerable<TestEntity> entities, Action<object[]> onGetTableOrRowSource, WhenCheckedParameters whenChecked)
            {
                _entities = entities;
                _onGetTableOrRowSource = onGetTableOrRowSource;
                _whenChecked = whenChecked;
            }
            public IDatabase GetDatabase(string schema)
            {
                return new TestSchema(_entities, _onGetTableOrRowSource, _whenChecked);
            }

            public IVariable GetVariable(string name)
            {
                throw new NotImplementedException();
            }

            public void SetVariable<T>(string name, T value)
            {
                throw new NotImplementedException();
            }

            public void SetVariable<T>(string database, string schema, string name, T value)
            {
                throw new NotImplementedException();
            }

            public void SetVariable(string name, Type type, object value)
            {
                throw new NotImplementedException();
            }

            public void SetVariable(string database, string schema, string name, Type type, object value)
            {
                throw new NotImplementedException();
            }
        }

        private class TestSchema : BaseDatabase
        {

            private readonly IEnumerable<TestEntity> _entities;
            private readonly Action<object[]> _onGetTableOrRowSource;
            private readonly WhenCheckedParameters _whenChecked;

            public TestSchema(IEnumerable<TestEntity> entities, Action<object[]> onGetTableOrRowSource,
                WhenCheckedParameters whenChecked)
                : base("test")//, CreateLibrary())
            {
                _entities = entities;
                _onGetTableOrRowSource = onGetTableOrRowSource;
                _whenChecked = whenChecked;
            }

            //public override RowSource GetRowSource(string schema, string name, RuntimeContext communicator, params object[] parameters)
            //{
            //    if(_whenChecked == WhenCheckedParameters.OnSchemaTableOrRowSourceGet) _onGetTableOrRowSource(parameters);
            //    return new EntitySource<TestEntity>(_entities);
            //}

            public override ITable GetTableByName(string schema, string name)
            {
                if (_whenChecked == WhenCheckedParameters.OnSchemaTableOrRowSourceGet) _onGetTableOrRowSource(new object[0]);
                return new TestTable();
            }

            //private static MethodsAggregator CreateLibrary()
            //{
            //    var methodManager = new MethodsManager();
            //    //var propertiesManager = new PropertiesManager();

            //    var lib = new TestLibrary();

            //    //propertiesManager.RegisterProperties(lib);
            //    methodManager.RegisterLibraries(lib);

            //    return new MethodsAggregator(methodManager);//, propertiesManager);
            //}

            //public override SchemaMethodInfo[] GetConstructors(string schema)
            //{
            //    var methodInfos = new List<SchemaMethodInfo>();
            //    return methodInfos.ToArray();
            //}
        }

        private class TestTable : ITable
        {
            public IColumn[] Columns => new IColumn[0];

            public string Name { get; }

            public string Schema { get; }
        }

        private class TestEntity
        {
        }

        private CompiledQuery CreateAndRunVirtualMachine(string script, IEnumerable<TestEntity> source, Action<object[]> onGetTableOrRowSource, WhenCheckedParameters whenChecked)
        {
            return InstanceCreator.CompileForExecution(script, new TestSchemaProvider(source, onGetTableOrRowSource, whenChecked));
        }
    }
}
