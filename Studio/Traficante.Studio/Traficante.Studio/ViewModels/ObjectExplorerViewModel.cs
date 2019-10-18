using Dock.Model.Controls;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using Traficante.Studio.Models;
using Traficante.Studio.Services;
using Traficante.Studio.Views;

namespace Traficante.Studio.ViewModels
{
    public class ObjectExplorerViewModel : ToolTab
    {
        public ReactiveCommand<Unit, Unit> ConnectToSqlServerCommand { get; }
        public ObservableCollection<ObjectModel> Objects => ((AppData)this.Context).Objects;

        public ObjectExplorerViewModel()
        {
            ConnectToSqlServerCommand = ReactiveCommand.Create<Unit, Unit>(ConnectToSqlServer);
            
        }

        private Unit ConnectToSqlServer(Unit arg)
        {
            Interactions.ConnectToSqlServer.Handle(null)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(x =>
                {
                    if (x != null)
                    {
                        var model = new SqlServerObjectModel(x);
                        ((AppData)this.Context).Objects.Add(model);
                    }
                });
            return Unit.Default;
        }
    }
}
