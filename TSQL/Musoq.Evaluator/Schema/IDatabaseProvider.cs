namespace Musoq.Schema
{
    public interface IDatabaseProvider
    {
        IDatabase GetDatabase(string database);
    }
}