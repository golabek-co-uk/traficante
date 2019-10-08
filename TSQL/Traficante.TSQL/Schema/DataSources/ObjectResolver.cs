using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Traficante.TSQL.Schema.DataSources
{
    public interface IObjectResolver
    {
        object GetValue(string name);
    }

    public class EntityResolver<T> : IObjectResolver
    {
        private readonly T _entitiy;
        private readonly IDictionary<int, Func<T, object>> _indexToObjectAccessMap;
        private readonly IDictionary<string, int> _nameToIndexMap;

        public EntityResolver(T entity, IDictionary<string, int> nameToIndexMap,
            IDictionary<int, Func<T, object>> indexToObjectAccessMap)
        {
            _entitiy = entity;
            _nameToIndexMap = nameToIndexMap;
            _indexToObjectAccessMap = indexToObjectAccessMap;
        }

        public object GetValue(string name)
        {
            return _indexToObjectAccessMap[_nameToIndexMap[name]](_entitiy);
        }
    }

    public class DictionaryResolver : Dictionary<string, object>, IObjectResolver
    {
        public DictionaryResolver()
        {
        }

        public DictionaryResolver(IDictionary<string, object> entity) : base(entity)
        {
        }

        public object GetValue(string name)
        {
            return this[name];
        }
    }

    public class ObjectResolver : IObjectResolver
    {
        private object obj;

        public ObjectResolver()
        {
        }

        public ObjectResolver(object obj)
        {
            this.obj = obj;
        }

        public object GetValue(string name)
        {
            return this.obj.GetType().GetField(name).GetValue(this.obj);
        }
    }
}