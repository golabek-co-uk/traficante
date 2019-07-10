using System;
using System.Collections.Generic;
using System.Text;

namespace Traficante.TSQL.Parser
{
    public enum QueryPart
    {
        None,
        Select,
        From,
        Where,
        GroupBy,
        Having
    }
}
