using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Traficante.TSQL.Plugins.Attributes;

namespace Traficante.TSQL.Plugins
{
    public partial class LibraryBase
    {
        
        [AggregationGetMethod]
        public string AggregateValues(string name)
        {
            return default(string);
        }

    }
}
