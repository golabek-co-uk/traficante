using Avalonia.Controls;
using Dock.Model.Controls;
using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using Traficante.Studio.Models;

namespace Traficante.Studio.ViewModels
{
    public class ObjectExplorerViewModel : Tool
    {
        
        public AppData AppData => (AppData)this.Context;

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
            AppData.Objects.Remove(sqlServer);
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
            AppData.Objects.Remove(mySql);
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
            AppData.Objects.Remove(sqlite);
        }

        public void GenerateSelectQuery(IObjectPath objectPath, IObjectFields objectFields)
        {
            var path = objectPath.GetObjectPath();
            var sqlPath = string.Join(".", path.Select(x => $"[{x}]"));
            var fields = objectFields.GetObjectFields();
            var sqlFields = fields.Length > 0 ? string.Join(".", fields.Select(x => $"[{x}]")) : "*";
            var sql = $"SELECT {sqlFields} FROM {sqlPath}";
            AppData.Queries.Add(new QueryModel { Id = Guid.NewGuid().ToString(), Text = sql });
        }
    }
}
