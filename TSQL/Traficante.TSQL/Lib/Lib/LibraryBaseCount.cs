using System;
using Traficante.TSQL.Lib.Attributes;

namespace Traficante.TSQL.Lib
{
    public partial class Library
    {
        [AggregationGetMethod]
        public int Count(object name)
        {
            return default(int);
        }

    }
}
