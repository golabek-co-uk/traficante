using System.Threading;
using Traficante.TSQL;

static public class TSQLEngineExtensions
{
    static public Traficante.TSQL.Tests.DataTable RunAndReturnTable(this TSQLEngine engine, string script, CancellationToken ct = default)
    {
        var result = engine.Run(script, ct);
        if (result == null)
            return null;
        return new Traficante.TSQL.Tests.DataTable(result);
    }
}
