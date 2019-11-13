using System;
using System.Collections.Generic;
using Avalonia.Data;
using Dock.Avalonia.Controls;
using Dock.Model;
using Dock.Model.Controls;
using Traficante.Studio.Views;
using Traficante.Studio.ViewModels;
using System.Reactive;
using ReactiveUI.Legacy;
using ReactiveUI;
using System.Collections.Specialized;
using System.Collections.ObjectModel;
using Traficante.Studio.Models;
using System.Linq;
using System.Reactive.Linq;
using DynamicData.Binding;
using DynamicData;

namespace Traficante.Studio
{
    public class MainWindowDockFactory : DockFactory
    {
        private AppData _context;

        public MainWindowDockFactory(AppData context)
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

            _context
                .Queries
                .ToObservableChangeSet()
                .Transform(x =>
                {
                    return new QueryViewModel(x.Id, "New query", _context);
                })
                .ObserveOn(RxApp.MainThreadScheduler)
                .ActOnEveryObject(
                    added =>
                    {
                        bool exists = documentsWindows.Views
                            .Select(x => (QueryViewModel)x)
                            .Any(x => x.Id == added.Id);
                        if (exists == false)
                        {
                            if (documentsWindows.CurrentView != null)
                            {
                                documentsWindows.Views.Insert(
                                    documentsWindows.Views.IndexOf(documentsWindows.CurrentView) + 1,
                                    added);
                            }
                            else
                            {
                                documentsWindows.Views.Add(added);
                            }
                            this.InitLayout(added);
                            this.InitLayout(documentsWindows);
                            documentsWindows.CurrentView = added;
                        }
                    },
                    removed =>
                    {
                        bool exists = documentsWindows.Views
                            .Select(x => (QueryViewModel)x)
                            .Any(x => x.Id == removed.Id);
                        if (exists)
                            documentsWindows.Views.Remove(removed);
                    }
                );

            ((INotifyCollectionChanged)documentsWindows.Views).CollectionChanged += new NotifyCollectionChangedEventHandler(
                (object sender, NotifyCollectionChangedEventArgs e) =>
                {
                    if (e.OldItems != null)
                    {
                        foreach (QueryViewModel view in e.OldItems)
                        {
                            var queryModel = _context.Queries.FirstOrDefault(x => x.Id == view.Id);
                            if (queryModel != null)
                                _context.Queries.Remove(queryModel);
                        }
                    }
                });

            documentsWindows
                .WhenAnyValue(x => x.CurrentView)
                .Subscribe(x => {
                    if (x != null)
                        _context.SelectedQueryIndex = documentsWindows.Views.IndexOf(x);
                    else
                        _context.SelectedQueryIndex = -1;
                });
            _context
                .WhenAnyValue(x => x.SelectedQueryIndex)
                .Subscribe(x => {
                    var view = documentsWindows.Views.ElementAtOrDefault(x);
                    if (view != null)
                        documentsWindows.CurrentView = view;
                });

            Interactions.NewQuery.RegisterHandler(x =>
            {
                _context.Queries.Add(new QueryModel
                {
                    Id = Guid.NewGuid().ToString()
                });
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
