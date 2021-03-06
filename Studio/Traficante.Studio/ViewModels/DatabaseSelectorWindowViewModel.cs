﻿using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Traficante.Studio.Models;
using Traficante.Connect;

namespace Traficante.Studio.ViewModels
{
    public class DatabaseSelectorWindowViewModel : ViewModelBase
    {
        public AppData AppData { get; set; }
        public ObjectModel Output { get; set; }
        
        public ReactiveCommand<Unit, Unit> OkCommand { get; }
        public ReactiveCommand<Unit, Unit> CancelCommand { get; }
        public Interaction<Unit, Unit> CloseInteraction { get; } = new Interaction<Unit, Unit>();

        public IEnumerable<ISelectableObject> Objects { get; set; }

        [Reactive]
        public ISelectableObject SelectedObject { get; set; }

        [Reactive]
        public QueryLanguage SelectedQueryLanguage { get; set; }

        public DatabaseSelectorWindowViewModel(AppData appData)
        {
            AppData = appData;

            Objects =
                new List<ISelectableObject>() { new NotSelectedDatabaseModel() }
                .Concat(
                    this.AppData.Objects.Select<ObjectModel, ISelectableObject>(objectModel =>
                    {
                        if (objectModel is IDataSourceObjectModel dataSourceObject)
                            return new SelectableObjectModel(dataSourceObject);
                        return null;
                    }))
                .Where(x => x != null);

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
            this.AppData.GetSelectedQuery().SelectedLanguageId = SelectedObject.QueryLanguage.Id;

            List<string> objPath = new List<string>();
            IObjectModel obj = SelectedObject.Object;
            while (obj != null)
            {
                objPath.Add(obj.Title);
                obj = obj.Parent;
            }
            this.AppData.GetSelectedQuery().SelectedObjectPath = objPath.ToArray();

            await CloseInteraction.Handle(Unit.Default);
            return Unit.Default;
        }
    }
}
