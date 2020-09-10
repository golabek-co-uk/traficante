using System.Collections.ObjectModel;

namespace Traficante.Studio.Models
{
    public interface IObjectModel
    {
        string Title { get; }
        object Icon { get; }
        ObservableCollection<object> Items { get; }
    }
}
