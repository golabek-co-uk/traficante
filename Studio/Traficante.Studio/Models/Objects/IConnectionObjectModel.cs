using Traficante.Connect;

namespace Traficante.Studio.Models
{
    public interface IConnectionObjectModel
    {
        public string ConnectionAlias { get; }
        public ConnectorConfig ConnectorConfig { get; }
    }
}
