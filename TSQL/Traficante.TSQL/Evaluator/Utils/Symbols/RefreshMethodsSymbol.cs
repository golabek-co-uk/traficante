using System.Collections.Generic;
using System.Linq;
using Traficante.TSQL.Parser.Nodes;

namespace Traficante.TSQL.Evaluator.Utils.Symbols
{
    public class RefreshMethodsSymbol : Symbol
    {
        public RefreshMethodsSymbol(IEnumerable<AccessMethodNode> refreshMethods)
        {
            RefreshMethods = refreshMethods.ToArray();
        }

        public IReadOnlyList<AccessMethodNode> RefreshMethods { get; }
    }
}