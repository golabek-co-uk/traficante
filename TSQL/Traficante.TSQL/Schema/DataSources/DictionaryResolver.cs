using System.Collections.Generic;
using System.Linq.Expressions;

namespace Traficante.TSQL.Schema.DataSources
{
    public class DictionaryResolver : Dictionary<string, object>, IObjectResolver
    {
        public DictionaryResolver()
        {
        }
        //public DictionaryResolver(IEnumerable<string> names, IEnumerable<object> values)
        //{
        //}

        public DictionaryResolver(IDictionary<string, object> entity) : base(entity)
        {
        }

        public object Context => this;

        public IEnumerable<IObjectResolver> Stream { get; set; }

        object IObjectResolver.this[string name] => this[name];

        object IObjectResolver.this[int index] => null;

        public bool HasColumn(string name)
        {
            return this.ContainsKey(name);
        }

        object IObjectResolver.GetValue(string name)
        {
            return ((IObjectResolver)this)[name];
        }

        public override bool Equals(object obj)
        {
            return true;
        }


        public override int GetHashCode()
        {
            var hashCode = -1000162029;

            foreach(var key in this.Keys)
            {
                hashCode = hashCode * -1521134295 + 
                    (this.ContainsKey(key) ? this[key].GetHashCode() : 0);
            }

            return hashCode;
        }

    }

    public class AnonymousTypeResolver : IObjectResolver
    {
        private object obj;

        public AnonymousTypeResolver()
        {
        }
        //public DictionaryResolver(IEnumerable<string> names, IEnumerable<object> values)
        //{
        //}

        public AnonymousTypeResolver(object obj)
        {
            this.obj = obj;
        }

        public object Context => this;

        public IEnumerable<IObjectResolver> Stream { get; set; }

        object IObjectResolver.this[string name] => this.obj.GetType().GetField(name).GetValue(this.obj);// this[name];

        object IObjectResolver.this[int index] => null;

        public bool HasColumn(string name)
        {
            return this.obj.GetType().GetField(name) != null;
        }

        object IObjectResolver.GetValue(string name)
        {
            return ((IObjectResolver)this)[name];
        }



    }
}