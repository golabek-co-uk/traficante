namespace Traficante.TSQL.Schema
{
    public interface IEngine
    {
        IDatabase GetDatabase(string database);
        IVariable GetVariable(string name);
    }
}