using System.Diagnostics;
using System.Threading;
using Traficante.TSQL.Evaluator.Tables;

namespace Traficante.TSQL.Evaluator
{
    [DebuggerStepThrough]
    public class CompiledQuery
    {
        private readonly IRunnable _runnable;

        public CompiledQuery(IRunnable runnable)
        {
            _runnable = runnable;
        }

        public Table Run()
        {
            using(var exitSourcesLoaderTokenSource = new CancellationTokenSource())
            {
                var table = Run(exitSourcesLoaderTokenSource.Token);
                exitSourcesLoaderTokenSource.Cancel();
                return table;
            }
        }

        public Table Run(CancellationToken token)
        {
            return _runnable.Run(token);
        }
    }
}
