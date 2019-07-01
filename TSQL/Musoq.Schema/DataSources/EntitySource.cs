using System;
using System.Collections.Generic;

namespace Musoq.Schema.DataSources
{
    public class EntitySource<T> : RowSource
    {
        private readonly IEnumerable<T> _entities;
        private readonly IDictionary<int, Func<T, object>> _indexToObjectAccessMap;
        private readonly IDictionary<string, int> _nameToIndexMap;

        public EntitySource(IEnumerable<T> entities)
        {
            _entities = entities;
            _nameToIndexMap = new Dictionary<string, int>();
            _indexToObjectAccessMap = new Dictionary<int, Func<T, object>>();

            var props = typeof(T).GetProperties();
            for (int i = 0; i < props.Length; i++)
            {
                var prop = props[i];
                _nameToIndexMap.Add(prop.Name, i + 1);
                _indexToObjectAccessMap.Add(i + 1, x => prop.GetValue(x));
            }
        }

        public override IEnumerable<IObjectResolver> Rows
        {
            get
            {
                foreach (var item in _entities)
                    yield return new EntityResolver<T>(item, _nameToIndexMap, _indexToObjectAccessMap);
            }
        }
    }
}