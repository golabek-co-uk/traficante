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

        public void GenerateSelectQuery(IObjectSource objectPath)
        {
            var path = objectPath.GetObjectPath();
            var sqlPath = string.Join(".", path.Select(x => $"[{x}]"));
            var fields = objectPath.GetObjectFields();
            var sqlFields = fields.Length > 0 ? string.Join(".", fields.Select(x => $"[{x}]")) : "*";
            var sql = $"SELECT {sqlFields} FROM {sqlPath}";
            AppData.Queries.Add(new QueryModel { Id = Guid.NewGuid().ToString(), Text = sql });
        }

        public async void DragObjectPath(IObjectSource objectSource, IObjectField objectField, PointerPressedEventArgs e)
        {
            if (objectSource != null)
            {
                var path = objectSource.GetObjectPath();
                var sqlPath = string.Join(",", path.Select(x => $"[{x}]"));
                DataObject dragData = new DataObject();
                dragData.Set(DataFormats.Text, sqlPath);
                await DragDrop.DoDragDrop(e, dragData, DragDropEffects.Copy);
            }
            if (objectField != null)
            {
                var name = objectField.GetObjectFieldName();
                var sqlName = $"[{name}]";
                DataObject dragData = new DataObject();
                dragData.Set(DataFormats.Text, sqlName);
                await DragDrop.DoDragDrop(e, dragData, DragDropEffects.Copy);
            }
        }
    }
}
