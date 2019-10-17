using System;
using System.Collections.Generic;
using Avalonia.Data;
using Dock.Avalonia.Controls;
using Dock.Model;
using Dock.Model.Controls;
using Traficante.Studio.Views;
using Traficante.Studio.ViewModels;

namespace Traficante.Studio
{
    public class MainDockFactory : DockFactory
    {
        private object _context;

        public MainDockFactory(object context)
        {
            _context = context;
        }

        public override IDock CreateLayout()
        {
            var objectExplorer = new ObjectExplorerViewModel
            {
                Id = "ObjectExplorer",
                Title = "ObjectExplorer",
            };

            var queryWindows = new DocumentDock
            {
                Id = "DocumentsPane",
                Title = "DocumentsPane",
                Proportion = double.NaN,
                Views = CreateList<IView>()
            };

            objectExplorer.ConnectToSqlServerCommand.Subscribe(x =>
            {
                var queryWindow = new QueryWindowViewModel()
                {
                    Id = "QueryWindow",
                    Title = "New query"
                };
                if (queryWindows.CurrentView != null)
                {
                    queryWindows.Views.Insert(
                        queryWindows.Views.IndexOf(queryWindows.CurrentView) + 1,
                        queryWindow);
                }
                else
                {
                    queryWindows.Views.Add(queryWindow);
                }
                queryWindows.CurrentView = queryWindow;
            });

            var mainLayout = new LayoutDock
            {
                Id = "MainLayout",
                Title = "MainLayout",
                Proportion = double.NaN,
                Orientation = Orientation.Horizontal,
                CurrentView = null,
                Views = CreateList<IView>
                (
                    new LayoutDock
                    {
                        Id = "LeftPane",
                        Title = "LeftPane",
                        Proportion = double.NaN,
                        Orientation = Orientation.Vertical,
                        CurrentView = null,
                        Views = CreateList<IView>
                        (
                            new ToolDock
                            {
                                Id = "LeftPaneTop",
                                Title = "LeftPaneTop",
                                Proportion = double.NaN,
                                CurrentView = objectExplorer,
                                Views = CreateList<IView>
                                (
                                    objectExplorer
                                )
                            }
                        )
                    },
                    new SplitterDock()
                    {
                        Id = "LeftSplitter",
                        Title = "LeftSplitter"
                    },
                    queryWindows
                )
            };

            var mainView = new MainViewModel
            {
                Id = "Main",
                Title = "Main",
                CurrentView = mainLayout,
                Views = CreateList<IView>(mainLayout)
            };

            var root = CreateRootDock();

            root.Id = "Root";
            root.Title = "Root";
            root.CurrentView = mainView;
            root.DefaultView = mainView;
            root.Views = CreateList<IView>(mainView);
            root.Top = CreatePinDock();
            root.Top.Alignment = Alignment.Top;
            root.Bottom = CreatePinDock();
            root.Bottom.Alignment = Alignment.Bottom;
            root.Left = CreatePinDock();
            root.Left.Alignment = Alignment.Left;
            root.Right = CreatePinDock();
            root.Right.Alignment = Alignment.Right;

            return root;
        }

        public override void InitLayout(IView layout)
        {
            this.ContextLocator = new Dictionary<string, Func<object>>
            {
                [nameof(IRootDock)] = () => _context,
                [nameof(IPinDock)] = () => _context,
                [nameof(ILayoutDock)] = () => _context,
                [nameof(IDocumentDock)] = () => _context,
                [nameof(IToolDock)] = () => _context,
                [nameof(ISplitterDock)] = () => _context,
                [nameof(IDockWindow)] = () => _context,
                [nameof(IDocumentTab)] = () => _context,
                [nameof(IToolTab)] = () => _context,
                ["ObjectExplorer"] = () => _context,
                ["QueryWindow"] = () => _context,
                ["LeftPane"] = () => _context,
                ["LeftPaneTop"] = () => _context,
                ["LeftSplitter"] = () => _context,
                ["DocumentsPane"] = () => _context,
                ["MainLayout"] = () => _context,
                ["Main"] = () => _context,
            };

            this.HostLocator = new Dictionary<string, Func<IDockHost>>
            {
                [nameof(IDockWindow)] = () =>
                {
                    var hostWindow = new HostWindow()
                    {
                        [!HostWindow.TitleProperty] = new Binding("CurrentView.Title")
                    };

                    hostWindow.Content = new DockControl()
                    {
                        [!DockControl.LayoutProperty] = hostWindow[!HostWindow.DataContextProperty]
                    };

                    return hostWindow;
                }
            };

            base.InitLayout(layout);
        }
    }
}
