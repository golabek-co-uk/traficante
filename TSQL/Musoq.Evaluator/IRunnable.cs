using System.Threading;
using Musoq.Evaluator.Tables;
using Musoq.Schema;

namespace Musoq.Evaluator
{
    public interface IRunnable
    {
        IDatabaseProvider Provider { get; set; }
        Table Run(CancellationToken token);
    }
}