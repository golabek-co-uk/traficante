using System.Threading;
using Traficante.TSQL.Evaluator.Tables;
using Traficante.TSQL.Schema;

namespace Traficante.TSQL.Evaluator
{
    public interface IRunnable
    {
        ISchemaProvider Provider { get; set; }
        Table Run(CancellationToken token);
    }
}