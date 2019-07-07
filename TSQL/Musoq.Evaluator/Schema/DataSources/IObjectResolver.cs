using System.Collections.Generic;

namespace Traficante.TSQL.Schema.DataSources
{
    public interface IObjectResolver
    {
        object Context { get; }

        object this[string name] { get; }

        object this[int index] { get; }

        object GetValue(string name);

        bool HasColumn(string name);

        IEnumerable<IObjectResolver> Stream { get; set; }
    }
}