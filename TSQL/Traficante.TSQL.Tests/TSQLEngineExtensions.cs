using Traficante.TSQL;

static public class TSQLEngineExtensions
{
    static public Traficante.TSQL.Tests.DataTable RunAndReturnTable(this TSQLEngine engine, string script)
    {
        var result = engine.Run(script);
        if (result == null)
            return null;
        return new Traficante.TSQL.Tests.DataTable(result);
    }
}
