using System.Collections.Generic;
using Traficante.TSQL.Schema.DataSources;

namespace Traficante.TSQL.Evaluator.Tables
{
    internal class TransientVariableSource : RowSource
    {
        public TransientVariableSource(string name)
        {
        }

        public override IEnumerable<IObjectResolver> Rows { get; }
    }
}