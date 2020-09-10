using System.Collections.ObjectModel;
using Traficante.Studio.ViewModels;

namespace Traficante.Studio.Models
{
    public interface IQueryableObjectModel : IObjectModel
    {
        QueryLanguageModel[] QueryLanguages { get; }
        ObservableCollection<IObjectModel> QueryableChildren { get; }
    }
}
