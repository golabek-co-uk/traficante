using Dock.Model.Controls;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using Traficante.Studio.Models;

namespace Traficante.Studio.ViewModels
{
    public class MainWindowToolBarViewModel : ViewModelBase
    {
        public object Context { get; set; }

        public ReactiveCommand<Unit, Unit> ConnectToSqlServerCommand { get; }
        public ReactiveCommand<Unit, Unit> NewQueryCommand { get; }

        public AppData AppData => ((AppData)this.Context);

        public MainWindowToolBarViewModel()
        {
            ConnectToSqlServerCommand = ReactiveCommand.Create<Unit, Unit>(ConnectToSqlServer);
            NewQueryCommand = ReactiveCommand.Create<Unit, Unit>(NewQuery);
        }

        private Unit NewQuery(Unit arg)
        {
            Interactions.NewQuery.Handle(Unit.Default)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe();

            return Unit.Default;
        }

        private Unit ConnectToSqlServer(Unit arg)
        {
            Interactions.ConnectToSqlServer.Handle(null)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe();

            return Unit.Default;
        }


    }
}
