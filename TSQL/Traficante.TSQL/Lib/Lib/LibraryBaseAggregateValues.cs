using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Traficante.TSQL.Lib.Attributes;

namespace Traficante.TSQL.Lib
{
    public partial class Library
    {
        
        [AggregationGetMethod]
        public string AggregateValues(string name)
        {
            return default(string);
        }

    }
}
