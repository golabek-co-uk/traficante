using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Traficante.Studio.Models;

namespace Traficante.Studio.ViewModels
{
    public class DatabaseSelectorWindowViewModel : ViewModelBase
    {
        public AppData AppData { get; set; }
        public ObjectModel Output { get; set; }
        
        public ReactiveCommand<Unit, Unit> OkCommand { get; }
        public ReactiveCommand<Unit, Unit> CancelCommand { get; }
        public Interaction<Unit, Unit> CloseInteraction { get; } = new Interaction<Unit, Unit>();

        public IEnumerable<object> Objects
        {
            get
            {
                var objects = 
                    new List<ISelectableObject>() { new NotSelectedDatabaseModel() }
                    .Concat(
                        this.AppData.Objects.Select<ObjectModel, ISelectableObject>(objectModel =>
                        {
                            if (objectModel is IQueryableObjectModel)
                            {
                                var queryableObject = (IQueryableObjectModel)objectModel;
                                return new SelectableObjectModel(queryableObject);
                            }
                            return null;
                        }))
                    .Where(x => x != null);
                return objects;
            }
        }

        [Reactive]
        public ObjectModel SelectedObject { get; set; }

        [Reactive]
        public QueryLanguageModel SelectedQueryLanguage { get; set; }

        public DatabaseSelectorWindowViewModel(AppData appData)
        {
            AppData = appData;

            OkCommand = ReactiveCommand
                .CreateFromTask(
                    async () => await Ok(),
                    this.WhenAnyValue(x => x.SelectedObject).Select(x => x != null));

            CancelCommand = ReactiveCommand
                .CreateFromTask(async () => await Cancel());
        }

        private async Task<Unit> Cancel()
        {
            await CloseInteraction.Handle(Unit.Default);
            return Unit.Default;
        }

        private async Task<Unit> Ok()
        {
            this.AppData.GetSelectedQuery().LanguageId = ((ISelectableObject)SelectedObject).QueryLanguage.Id;
            await CloseInteraction.Handle(Unit.Default);
            return Unit.Default;
        }
    }
}
