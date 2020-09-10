using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Generators;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Parsers;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.ReactiveUI;
using Org.BouncyCastle.Asn1.X509;
using ReactiveUI;
using SharpDX.Direct2D1;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
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
                this.OneWayBind(ViewModel, x => x.AppData.Objects, x => x.Objects.Items, x => (System.Collections.IEnumerable)x)
                    .DisposeWith(disposables);

                Objects.ItemContainerGenerator.Index.Materialized += TreeViewItemMaterialized;
            });
            AvaloniaXamlLoader.Load(this);
        }

        private void TreeViewItemMaterialized(object sender, ItemContainerEventArgs e)
        {
            var item = (TreeViewItem)e.Containers[0].ContainerControl;
            //var model = item.DataContext as ObjectModel;

            //var iconResource = Objects.FindResource(model.Icon);
            //var icon = item.FindControl<DrawingPresenter>("Icon");
            //icon.Drawing = iconResource as Drawing;

            if (item.DataContext is LoadChildrenObjectModel)
            {
                item.IsVisible = false;
            }
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
            if (item.DataContext is ElasticSearchObjectModel elastic)
            {
                MenuItem change = new MenuItem { Header = "Change" };
                change.Click += (x, y) => ViewModel.ChangeObject(elastic);
                MenuItem remove = new MenuItem { Header = "Remove" };
                remove.Click += (x, y) => ViewModel.RemoveObject(elastic);
                item.ContextMenu = item.ContextMenu != null ? item.ContextMenu : new ContextMenu();
                item.ContextMenu.Items = new List<MenuItem> { change, remove };
            }
            if (item.DataContext is FilesObjectModel file)
            {
                MenuItem change = new MenuItem { Header = "Change" };
                change.Click += (x, y) => ViewModel.ChangeObject(file);
                MenuItem remove = new MenuItem { Header = "Remove" };
                remove.Click += (x, y) => ViewModel.RemoveObject(file);
                item.ContextMenu = item.ContextMenu != null ? item.ContextMenu : new ContextMenu();
                item.ContextMenu.Items = new List<MenuItem> { change, remove };
            }
            if (item.DataContext is ITableObjectModel objectPath)
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

            if (item.DataContext is ITableObjectModel objectPath2 || item.DataContext is IFieldObjectModel objectField2)
            {
                item.PointerMoved += Item_PointerMoved;
            }
        }

        private TreeViewItem _pressedItem = null;

        private async void Item_PointerMoved(object sender, PointerEventArgs e)
        {
            if (e.InputModifiers == InputModifiers.LeftMouseButton)
            {
                if (this._pressedItem == null)
                {
                    this._pressedItem = sender as TreeViewItem;
                    var objectSource = _pressedItem.DataContext as ITableObjectModel;
                    var objectField = _pressedItem.DataContext as IFieldObjectModel;
                    if (objectSource != null)
                    {
                        var path = objectSource.TablePath;
                        var sqlPath = string.Join(".", path.Select(x => $"[{x}]"));
                        var pressedItemDraggedData = new DataObject();
                        pressedItemDraggedData.Set(DataFormats.Text, sqlPath);
                        await DragDrop.DoDragDrop(e, pressedItemDraggedData, DragDropEffects.Copy);
                    }
                    if (objectField != null)
                    {
                        var name = objectField.FieldName;
                        var sqlName = $"[{name}]";
                        var pressedItemDraggedData = new DataObject();
                        pressedItemDraggedData.Set(DataFormats.Text, sqlName);
                        await DragDrop.DoDragDrop(e, pressedItemDraggedData, DragDropEffects.Copy);
                    }

                }
            }
            else
            {
                this._pressedItem = null;
            }

        }

    }
}
