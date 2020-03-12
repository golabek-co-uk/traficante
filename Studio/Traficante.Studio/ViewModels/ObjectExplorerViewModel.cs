using Dock.Model.Controls;
using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.Reactive.Linq;
using Traficante.Studio.Models;

namespace Traficante.Studio.ViewModels
{
    public class ObjectExplorerViewModel : Tool
    {
        
        public ObservableCollection<ObjectModel> Objects => ((AppData)this.Context).Objects;

        public ObjectExplorerViewModel()
        {
        }

        public void ChangeObject(SqlServerObjectModel sqlServer)
        {
            Interactions.ConnectToSqlServer
                .Handle(sqlServer)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe();
        }

        public void RemoveObject(SqlServerObjectModel sqlServer)
        {
            Objects.Remove(sqlServer);
        }

        public void ChangeObject(MySqlObjectModel mySql)
        {
            Interactions.ConnectToMySql
                .Handle(mySql)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe();
        }

        public void RemoveObject(MySqlObjectModel mySql)
        {
            Objects.Remove(mySql);
        }

        public void ChangeObject(SqliteObjectModel sqlite)
        {
            Interactions.ConnectToSqlite
                .Handle(sqlite)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe();
        }

        public void RemoveObject(SqliteObjectModel sqlite)
        {
            Objects.Remove(sqlite);
        }
    }
}
