namespace Traficante.TSQL.Schema
{
    public interface ISchemaProvider
    {
        ISchema GetDatabase(string database);
    }
}