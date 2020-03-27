using Avalonia.Controls;
using Avalonia.Input;
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
                .Subscribe();
        }

        public void RemoveObject(SqlServerObjectModel sqlServer)
        {
            AppData.Objects.Remove(sqlServer);
        }

        public void ChangeObject(ElasticSearchObjectModel elastic)
        {
            Interactions.ConnectToElasticSearch
                .Handle(elastic)
                .Subscribe();
        }

        public void RemoveObject(ElasticSearchObjectModel elastic)
        {
            AppData.Objects.Remove(elastic);
        }

        public void ChangeObject(MySqlObjectModel mySql)
        {
            Interactions.ConnectToMySql
                .Handle(mySql)
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
                .Subscribe();
        }

        public void RemoveObject(SqliteObjectModel sqlite)
        {
            AppData.Objects.Remove(sqlite);
        }

        public void GenerateSelectQuery(ITableObjectModel objectPath)
        {
            var path = objectPath.GetTablePath();
            var sqlPath = string.Join(".", path.Select(x => $"[{x}]"));
            var fields = objectPath.GetTableFields();
            var sqlFields = fields.Length > 0 ? string.Join(", ", fields.Select(x => $"[{x}]")) : "*";
            var sql = $"SELECT {sqlFields} FROM {sqlPath}";
            AppData.Queries.Add(new QueryModel { Id = Guid.NewGuid().ToString(), Text = sql });
        }


    }
}
