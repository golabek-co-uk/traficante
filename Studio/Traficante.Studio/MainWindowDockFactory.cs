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
    public class MainWindowDockFactory : Factory
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
                VisibleDockables = CreateList<IDockable>()
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
                        bool exists = documentsWindows.VisibleDockables
                            .Select(x => (QueryViewModel)x)
                            .Any(x => x.Id == added.Id);
                        if (exists == false)
                        {
                            if (documentsWindows.ActiveDockable != null)
                            {
                                documentsWindows.VisibleDockables.Insert(
                                    documentsWindows.VisibleDockables.IndexOf(documentsWindows.ActiveDockable) + 1,
                                    added);
                            }
                            else
                            {
                                documentsWindows.VisibleDockables.Add(added);
                            }
                            this.InitLayout(added);
                            this.InitLayout(documentsWindows);
                            documentsWindows.ActiveDockable = added;
                        }
                    },
                    removed =>
                    {
                        bool exists = documentsWindows.VisibleDockables
                            .Select(x => (QueryViewModel)x)
                            .Any(x => x.Id == removed.Id);
                        if (exists)
                            documentsWindows.VisibleDockables.Remove(removed);
                    }
                );

            ((INotifyCollectionChanged)documentsWindows.VisibleDockables).CollectionChanged += new NotifyCollectionChangedEventHandler(
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
                .WhenAnyValue(x => x.ActiveDockable)
                .Subscribe(x => {
                    if (x != null)
                        _context.SelectedQueryIndex = documentsWindows.VisibleDockables.IndexOf(x);
                    else
                        _context.SelectedQueryIndex = -1;
                });
            _context
                .WhenAnyValue(x => x.SelectedQueryIndex)
                .Subscribe(x => {
                    var view = documentsWindows.VisibleDockables.ElementAtOrDefault(x);
                    if (view != null)
                        documentsWindows.ActiveDockable = view;
                });

            Interactions.NewQuery.RegisterHandler(x =>
            {
                _context.Queries.Add(new QueryModel
                {
                    Id = Guid.NewGuid().ToString()
                });
                x.SetOutput(Unit.Default);
            });

            var bodyLayout = new ProportionalDock
            {
                Id = "BodyLayout",
                Title = "BodyLayout",
                Proportion = double.NaN,
                Orientation = Orientation.Horizontal,
                ActiveDockable = null,
                VisibleDockables = CreateList<IDockable>
                (
                    new ProportionalDock
                    {
                        Id = "LeftPane",
                        Title = "LeftPane",
                        Proportion = double.NaN,
                        Orientation = Orientation.Vertical,
                        ActiveDockable = null,
                        VisibleDockables = CreateList<IDockable>
                        (
                            new ToolDock
                            {
                                Id = "LeftPaneTop",
                                Title = "LeftPaneTop",
                                Proportion = double.NaN,
                                ActiveDockable = objectExplorer,
                                VisibleDockables = CreateList<IDockable>
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

            var root = CreateRootDock();
            root.Id = "Root";
            root.Title = "Root";
            root.ActiveDockable = bodyLayout;
            root.DefaultDockable = bodyLayout;
            root.VisibleDockables = CreateList<IDockable>(bodyLayout);
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

        public override void InitLayout(IDockable layout)
        {
            this.ContextLocator = new Dictionary<string, Func<object>>
            {
                [nameof(IRootDock)] = () => _context,
                [nameof(IPinDock)] = () => _context,
                [nameof(IProportionalDock)] = () => _context,
                [nameof(IDocumentDock)] = () => _context,
                [nameof(IToolDock)] = () => _context,
                [nameof(ISplitterDock)] = () => _context,
                [nameof(IDockWindow)] = () => _context,
                [nameof(IDocument)] = () => _context,
                [nameof(ITool)] = () => _context,
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
                ["MainWindowMenu"] = () => _context,
                ["MainWindowMenuViewModel"] = () => _context,
                ["MainWindowMenuView"] = () => _context,
                ["MainWindowToolBar"] = () => _context,
                
            };

            this.HostWindowLocator = new Dictionary<string, Func<IHostWindow>>
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
