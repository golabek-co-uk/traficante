namespace Traficante.TSQL.Schema
{
    public interface ITable
    {
        string Name { get; }
        string[] Path { get; }
        IColumn[] Columns { get; }
    }
}