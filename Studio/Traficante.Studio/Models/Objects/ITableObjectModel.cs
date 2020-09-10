namespace Traficante.Studio.Models
{
    public interface ITableObjectModel
    {
        string TableName { get; }
        string[] TablePath { get; }
        string[] TableFields { get; }
    }
}
