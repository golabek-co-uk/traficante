using System.Collections.ObjectModel;
using System.Linq;
using Traficante.Studio.Models;
using Traficante.Studio.Views;

namespace Traficante.Studio.ViewModels
{
    public class NotSelectedDatabaseModel : ObjectModel, ISelectableObject
    {
        public IQueryableObjectModel Object { get; set; }
        public QueryLanguageRadioButtonModel[] QueryLanguages { get; set; }
        public QueryLanguageModel QueryLanguage { get; set; }
        public override string Title => "Not Selected";
        public override object Icon => BaseLightIcons.Database;
        public override ObservableCollection<object> Items => null;

        public NotSelectedDatabaseModel()
        {
            QueryLanguage = QueryLanguageModel.TraficantSQL;
            QueryLanguages = new[] { new QueryLanguageRadioButtonModel(QueryLanguageModel.TraficantSQL, this) };
        }
    }
}
