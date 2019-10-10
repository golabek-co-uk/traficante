using System;
using Traficante.TSQL.Plugins.Attributes;

namespace Traficante.TSQL.Plugins
{
    public partial class LibraryBase
    {
        [AggregationGetMethod]
        public int Count(object name)
        {
            return default(int);
        }

    }
}
