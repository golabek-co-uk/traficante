using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Generators;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using ReactiveUI;
using System;
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
                item.ContextMenu = item.ContextMenu != null ? item.ContextMenu : new ContextMenu();
                item.ContextMenu.Items = new List<MenuItem> { change, remove };
            } 
            if (item.DataContext is MySqlObjectModel mySql)
            {
                MenuItem change = new MenuItem { Header = "Change" };
                change.Click += (x, y) => ViewModel.ChangeObject(mySql);
                MenuItem remove = new MenuItem { Header = "Remove" };
                remove.Click += (x, y) => ViewModel.RemoveObject(mySql);
                item.ContextMenu = item.ContextMenu != null ? item.ContextMenu : new ContextMenu();
                item.ContextMenu.Items = new List<MenuItem> { change, remove };
            }
            if (item.DataContext is SqliteObjectModel sqlite)
            {
                MenuItem change = new MenuItem { Header = "Change" };
                change.Click += (x, y) => ViewModel.ChangeObject(sqlite);
                MenuItem remove = new MenuItem { Header = "Remove" };
                remove.Click += (x, y) => ViewModel.RemoveObject(sqlite);
                item.ContextMenu = item.ContextMenu != null ? item.ContextMenu : new ContextMenu();
                item.ContextMenu.Items = new List<MenuItem> { change, remove };
            }
            if (item.DataContext is IObjectSource objectPath)
            {
                MenuItem generateSelect = new MenuItem { Header = "Select query" };
                generateSelect.Click += (x, y) => ViewModel.GenerateSelectQuery(objectPath);
                MenuItem generate = new MenuItem { Header = "Generate" };
                generate.Items = new List<MenuItem> { generateSelect };
                item.ContextMenu = item.ContextMenu != null ? item.ContextMenu : new ContextMenu();
                item.ContextMenu.Items = new List<MenuItem> { generate };
            }
            if (item.ContextMenu == null)
            {
                item.ContextMenu = new ContextMenu();
                item.ContextMenu.IsVisible = false;
            }

            if (item.DataContext is IObjectSource objectPath2 || item.DataContext is IObjectField objectField2)
            {
                item.PointerPressed += Item_DoDrag;
            }
        }

        private async void Item_DoDrag(object sender, PointerPressedEventArgs e)
        {
            if (e.MouseButton == MouseButton.Left)
            {
                var item = (TreeViewItem)sender;
                var objectSource = item.DataContext as IObjectSource;
                var objectField = item.DataContext as IObjectField;
                ViewModel.DragObjectPath(objectSource, objectField, e);
            }
        }
    }
}
