using ReactiveUI;
using SharpDX.Direct3D11;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Traficante.Connect;
using Traficante.Studio.Models;

namespace Traficante.Studio.ViewModels
{
    public class SelectableObjectModel : ObjectModel, ISelectableObject
    {
        public IDataSourceObjectModel Object { get; set; }
        public QueryLanguageRadioButtonModel[] QueryLanguages { get; set; }
        public QueryLanguage QueryLanguage { get; set; }
        public override string Title => Object.Title;
        public override object Icon => Object.Icon;


        public SelectableObjectModel(IDataSourceObjectModel dataSource) : base(null)
        {
            this.Object = dataSource;
            this.QueryLanguages = dataSource
                .QueryLanguages
                .Select(x => new QueryLanguageRadioButtonModel(x, this))
                .ToArray();
            this.QueryLanguage = this.QueryLanguages.FirstOrDefault()?.QueryLanguage;
            if (dataSource.HasQueryableChildren == false)
                this.Children.Clear();
        }

        public override void LoadChildren()
        {
            Observable
                .FromAsync(() => Task.Run(() =>
                {
                    return Object.QueryableChildren ?? new ObservableCollection<IObjectModel>();
                }))
                .SelectMany(x => x)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Catch<IObjectModel, Exception>(ex =>
                {
                    Interactions.Exceptions.Handle(ex).Subscribe();
                    return Observable.Empty<IObjectModel>();
                })
                .Subscribe(x => Children.Add(x));
        }
    }
}
