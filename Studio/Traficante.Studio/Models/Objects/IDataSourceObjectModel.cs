using System.Collections.ObjectModel;
using Traficante.Connect;

namespace Traficante.Studio.Models
{
    public interface IDataSourceObjectModel : IObjectModel
    {
        public string ConnectionAlias { get; }
        public ConnectorConfig ConnectorConfig { get; }
        QueryLanguage[] QueryLanguages { get; }
        ObservableCollection<IObjectModel> QueryableChildren { get; }
        public bool HasQueryableChildren { get; }
    }
}
