//using System.Collections.Generic;
//using Traficante.TSQL.Schema.DataSources;

//namespace Traficante.TSQL.Evaluator.Tables
//{
//    public class RowResolver : IObjectResolver
//    {
//        private readonly IDictionary<string, int> _nameToIndexMap;
//        private readonly Row _row;

//        public RowResolver(Row row, IDictionary<string, int> nameToIndexMap)
//        {
//            _row = row;
//            _nameToIndexMap = nameToIndexMap;
//        }

//        public object Context => _row;

//        public IEnumerable<IObjectResolver> Stream { get; set; }

//        public bool HasColumn(string name)
//        {
//            return _nameToIndexMap.ContainsKey(name);
//        }

//        object IObjectResolver.this[string name]
//        {
//            get
//            {
//#if DEBUG
//                if (!_nameToIndexMap.ContainsKey(name))
//                    throw new System.Exception(name);
                
//                return _row[_nameToIndexMap[name]];
//#else
//                return _row[_nameToIndexMap[name]];
//#endif
//            }
//        }

//        object IObjectResolver.GetValue(string name)
//        {
//            return ((IObjectResolver)this)[name];
//        }

//        object IObjectResolver.this[int index] => _row[index];
//    }
//}