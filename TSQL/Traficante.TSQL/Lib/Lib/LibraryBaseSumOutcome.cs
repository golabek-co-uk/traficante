using System;
using System.Collections.Generic;
using System.Text;
using Traficante.TSQL.Lib.Attributes;

namespace Traficante.TSQL.Lib
{
    public partial class Library
    {
        [AggregationGetMethod]
        public decimal? SumOutcome(decimal? name)
        {
            return default(decimal?);
        }
    }
}
