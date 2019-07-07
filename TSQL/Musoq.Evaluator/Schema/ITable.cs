namespace Traficante.TSQL.Schema
{
    public interface ITable
    {
        string Name { get; }
        string Schema { get; }
        IColumn[] Columns { get; }
    }
}