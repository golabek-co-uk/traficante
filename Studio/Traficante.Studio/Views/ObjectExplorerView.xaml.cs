using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Generators;
using Avalonia.Controls.Primitives;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using ReactiveUI;
using System.Collections;
using System.Collections.Generic;
using System.Reactive.Disposables;
using Traficante.Studio.Models;
using Traficante.Studio.ViewModels;

namespace Traficante.Studio.Views
{
    public class ObjectExplorerView : ReactiveUserControl<ObjectExplorerViewModel>
    {
        public TreeView Objects => this.FindControl<TreeView>("Objects");
        
        public ObjectExplorerView()
        {
            this.InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.WhenActivated(disposables => 
            {
                this.OneWayBind(ViewModel, x => x.AppData.Objects, x => x.Objects.Items, x => (System.Collections. IEnumerable)x)
                    .DisposeWith(disposables);
            });
            AvaloniaXamlLoader.Load(this);
            Objects.ItemContainerGenerator.Index.Materialized += TreeViewItemMaterialized;
        }

        private void TreeViewItemMaterialized(object sender, ItemContainerEventArgs e)
        {
            var item = (TreeViewItem)e.Containers[0].ContainerControl;
            if (item.DataContext is SqlServerObjectModel sqlServer)
            {
                MenuItem change = new MenuItem { Header = "Change" };
                change.Click += (x, y) => ViewModel.ChangeObject(sqlServer);
                MenuItem remove = new MenuItem { Header = "Remove" };
                remove.Click += (x, y) => ViewModel.RemoveObject(sqlServer);
                item.ContextMenu = new ContextMenu();
                item.ContextMenu.Items = new List<MenuItem> { change, remove };
            } else if (item.DataContext is MySqlObjectModel mySql)
            {
                MenuItem change = new MenuItem { Header = "Change" };
                change.Click += (x, y) => ViewModel.ChangeObject(mySql);
                MenuItem remove = new MenuItem { Header = "Remove" };
                remove.Click += (x, y) => ViewModel.RemoveObject(mySql);
                item.ContextMenu = new ContextMenu();
                item.ContextMenu.Items = new List<MenuItem> { change, remove };
            }
            else if (item.DataContext is SqliteObjectModel sqlite)
            {
                MenuItem change = new MenuItem { Header = "Change" };
                change.Click += (x, y) => ViewModel.ChangeObject(sqlite);
                MenuItem remove = new MenuItem { Header = "Remove" };
                remove.Click += (x, y) => ViewModel.RemoveObject(sqlite);
                item.ContextMenu = new ContextMenu();
                item.ContextMenu.Items = new List<MenuItem> { change, remove };
            }
            else if (item.DataContext is IObjectPath objectPath && item.DataContext is IObjectFields objectFields)
            {
                MenuItem generateSelect = new MenuItem { Header = "Select query" };
                generateSelect.Click += (x, y) => ViewModel.GenerateSelectQuery(objectPath, objectFields);
                MenuItem generate = new MenuItem { Header = "Generate" };
                generate.Items = new List<MenuItem> { generateSelect };
                item.ContextMenu = new ContextMenu();
                item.ContextMenu.Items = new List<MenuItem> { generate };
            }
            else
            {
                item.ContextMenu = new ContextMenu();
                item.ContextMenu.IsVisible = false;
            }
        }

        private void Remove_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            throw new System.NotImplementedException();
        }
    }
}
