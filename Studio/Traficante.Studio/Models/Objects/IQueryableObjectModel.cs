using System.Collections.ObjectModel;
using Traficante.Connect;
using Traficante.Studio.ViewModels;

namespace Traficante.Studio.Models
{
    public interface IQueryableObjectModel : IObjectModel
    {
        QueryLanguage[] QueryLanguages { get; }
        ObservableCollection<IObjectModel> QueryableChildren { get; }
    }
}
