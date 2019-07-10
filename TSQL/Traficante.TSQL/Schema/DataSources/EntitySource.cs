using Traficante.TSQL.Schema.Helpers;
using System;
using System.Collections.Generic;

namespace Traficante.TSQL.Schema.DataSources
{
    public class EntitySource<T> : RowSource
    {
        private readonly IEnumerable<T> _entities;
        private readonly IDictionary<int, Func<T, object>> _indexToObjectAccessMap;
        private readonly IDictionary<string, int> _nameToIndexMap;

        public EntitySource(EntityMap<T> entityMap, IEnumerable<T> entities)
        {
            _entities = entities;
            _nameToIndexMap = entityMap.NameToIndexMap;
            _indexToObjectAccessMap = entityMap.IndexToMethodAccessMap;
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