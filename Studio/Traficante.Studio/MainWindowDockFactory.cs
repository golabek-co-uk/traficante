using System;
using System.Collections.Generic;
using Avalonia.Data;
using Dock.Avalonia.Controls;
using Dock.Model;
using Dock.Model.Controls;
using Traficante.Studio.Views;
using Traficante.Studio.ViewModels;
using System.Reactive;

namespace Traficante.Studio
{
    public class MainWindowDockFactory : DockFactory
    {
        private object _context;

        public MainWindowDockFactory(object context)
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

            var documentsWindows = new DocumentDock
            {
                Id = "DocumentsPane",
                Title = "DocumentsPane",
                Proportion = double.NaN,
                Views = CreateList<IView>()
            };

            Interactions.NewQuery.RegisterHandler(x =>
            {
                var queryWindow = new QueryViewModel()
                {
                    Id = "Query",
                    Title = "New query"
                };

                if (documentsWindows.CurrentView != null)
                {
                    documentsWindows.Views.Insert(
                        documentsWindows.Views.IndexOf(documentsWindows.CurrentView) + 1,
                        queryWindow);
                }
                else
                {
                    documentsWindows.Views.Add(queryWindow);
                }
                documentsWindows.CurrentView = queryWindow;
                this.InitLayout(queryWindow);
                this.InitLayout(documentsWindows);
                x.SetOutput(Unit.Default);
            });

            var bodyLayout = new LayoutDock
            {
                Id = "BodyLayout",
                Title = "BodyLayout",
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
                    documentsWindows
                )
            };

            var bodyView = new MainWindowBodyViewModel
            {
                Id = "Main",
                Title = "Main",
                CurrentView = bodyLayout,
                Views = CreateList<IView>(bodyLayout)
            };

            var root = CreateRootDock();

            root.Id = "Root";
            root.Title = "Root";
            root.CurrentView = bodyView;
            root.DefaultView = bodyView;
            root.Views = CreateList<IView>(bodyView);
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
                ["Query"] = () => _context,
                ["QueryView"] = () => _context,
                ["QueryViewModel"] = () => _context,
                ["LeftPane"] = () => _context,
                ["LeftPaneTop"] = () => _context,
                ["LeftSplitter"] = () => _context,
                ["DocumentsPane"] = () => _context,
                ["BodyLayout"] = () => _context,
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
