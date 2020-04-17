using System;
using System.Collections.Generic;
using System.Text;
using Traficante.TSQL.Lib.Attributes;

namespace Traficante.TSQL.Lib
{
    public partial class Library
    {
        [AggregationGetMethod]
        public decimal? Sum(decimal? val)
        {
            return default(decimal?);
        }
    }
}
