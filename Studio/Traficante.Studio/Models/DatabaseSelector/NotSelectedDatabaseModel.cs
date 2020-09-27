using System.Collections.ObjectModel;
using System.Linq;
using Traficante.Connect;
using Traficante.Studio.Models;
using Traficante.Studio.Views;

namespace Traficante.Studio.ViewModels
{
    public class NotSelectedDatabaseModel : ObjectModel, ISelectableObject
    {
        public IDataSourceObjectModel Object { get; set; }
        public QueryLanguageRadioButtonModel[] QueryLanguages { get; set; }
        public QueryLanguage QueryLanguage { get; set; }
        public override string Title => "Not Selected";
        public override object Icon => BaseLightIcons.Database;
        public override ObservableCollection<IObjectModel> Children => null;

        public NotSelectedDatabaseModel() : base(null)
        {
            QueryLanguage = QueryLanguage.TraficantSQL;
            QueryLanguages = new[] { new QueryLanguageRadioButtonModel(QueryLanguage.TraficantSQL, this) };
        }
    }
}
