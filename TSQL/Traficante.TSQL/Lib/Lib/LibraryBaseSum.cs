using System;
using System.Collections.Generic;
using System.Text;
using Traficante.TSQL.Plugins.Attributes;

namespace Traficante.TSQL.Plugins
{
    public partial class LibraryBase
    {
        [AggregationGetMethod]
        public decimal Sum(decimal val)
        {
            return default(decimal);
        }
    }
}
