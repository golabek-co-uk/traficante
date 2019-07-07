//using Traficante.TSQL.Schema;
//using Traficante.TSQL.Schema.DataSources;
//using Traficante.TSQL.Schema.Helpers;
//using Traficante.TSQL.Schema.Reflection;
//using System.Collections.Generic;

//namespace Traficante.TSQL.Evaluator.TemporarySchemas
//{
//    public class DescSchema : SchemaBase
//    {
//        private readonly ISchemaColumn[] _columns;
//        private readonly ISchemaTable _table;

//        public DescSchema(string name, ISchemaTable table, ISchemaColumn[] columns)
//            : base(name, null)
//        {
//            _table = table;
//            _columns = columns;
//        }

//        public override ISchemaTable GetTableByName(string name, params object[] parameters)
//        {
//            return _table;
//        }

//        public override RowSource GetRowSource(string name, RuntimeContext interCommunicator, params object[] parameters)
//        {
//            return new TableMetadataSource(_columns);
//        }

//        public override SchemaMethodInfo[] GetConstructors()
//        {
//            var constructors = new List<SchemaMethodInfo>();

//            constructors.Add(new SchemaMethodInfo(nameof(TableMetadataSource), ConstructorInfo.Empty<TableMetadataSource>()));

//            return constructors.ToArray();
//        }
//    }
//}