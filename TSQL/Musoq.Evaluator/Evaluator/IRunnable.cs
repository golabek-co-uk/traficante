using System.Threading;
using Traficante.TSQL.Evaluator.Tables;
using Traficante.TSQL.Schema;

namespace Traficante.TSQL.Evaluator
{
    public interface IRunnable
    {
        IDatabaseProvider Provider { get; set; }
        Table Run(CancellationToken token);
    }
}