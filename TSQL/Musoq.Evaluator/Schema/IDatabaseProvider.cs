namespace Traficante.TSQL.Schema
{
    public interface IDatabaseProvider
    {
        IDatabase GetDatabase(string database);
    }
}