using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Traficante.Studio.Models;

namespace Traficante.Studio.ViewModels
{
    public class SelectableObjectModel : ObjectModel, ISelectableObject
    {
        public IQueryableObjectModel Object { get; set; }
        public QueryLanguageRadioButtonModel[] QueryLanguages { get; set; }
        public QueryLanguageModel QueryLanguage { get; set; }
        public override string Title => Object.Title;
        public override object Icon => Object.Icon;


        public SelectableObjectModel(IQueryableObjectModel queryableObject)
        {
            this.Object = queryableObject;
            this.QueryLanguages = queryableObject
                .QueryLanguages
                .Select(x => new QueryLanguageRadioButtonModel(x, this))
                .ToArray();
            this.QueryLanguage = this.QueryLanguages.FirstOrDefault()?.QueryLanguage;
        }

        public override void LoadItems()
        {
            Observable
                .FromAsync(() => Task.Run(() =>
                {
                    return Object.QueryableItems ?? new ObservableCollection<object>();
                }))
                .SelectMany(x => x)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Catch<object, Exception>(ex =>
                {
                    Interactions.Exceptions.Handle(ex).Subscribe();
                    return Observable.Empty<object>();
                })
                .Subscribe(x => Items.Add(x));
        }
    }
}
