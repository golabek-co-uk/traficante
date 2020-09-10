using System.Collections.ObjectModel;

namespace Traficante.Studio.Models
{
    public interface IObjectModel
    {
        string Title { get; }
        object Icon { get; }
        ObservableCollection<IObjectModel> Children { get; }
        IObjectModel Parent { get; }
    }
}
