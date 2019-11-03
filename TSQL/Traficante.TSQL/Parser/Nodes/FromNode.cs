using System;
using System.Collections;
using Traficante.TSQL.Schema.DataSources;

namespace Traficante.TSQL.Parser.Nodes
{
    public abstract class FromNode : Node
    {
        protected FromNode(string alias)
        {
            Alias = alias;
        }

        public virtual string Alias { get; }

        public override Type ReturnType => typeof(IEnumerable);
    }
}