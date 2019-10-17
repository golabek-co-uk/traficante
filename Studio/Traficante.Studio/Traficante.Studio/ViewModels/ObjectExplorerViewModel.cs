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
                        var model = new SqlServerObjectModel();
                        model.Name = "SqlServer";
                        model.ConnectionInfo = x;
                        ((AppData)this.Context).Objects.Add(model);
                    }
                    
                    //var items = new SqlServerService().GetSchema(x, CancellationToken.None).Result;
                    //items.ForEach(x => Objects.Add(x));
                });
            return Unit.Default;
        }
    }
}
