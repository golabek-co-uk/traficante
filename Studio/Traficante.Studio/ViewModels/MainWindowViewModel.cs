using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using Avalonia.Controls;
using Dock.Model;
using ReactiveUI;
using Traficante.Studio.Models;
using Traficante.Studio.Views;

namespace Traficante.Studio.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private IFactory _factory;
        private IDock _layout;
        private string _currentView;
        private AppData _appData;

        public MainWindowViewModel()
        {
            Interactions.ConnectToSqlServer.RegisterHandler(
                async interaction =>
                {
                    var dataContext = new ConnectToSqlServerWindowViewModel(interaction.Input, _appData);
                    var dialog = new ConnectToSqlServerWindow() { DataContext = dataContext };
                    await dialog.ShowDialog(Window);
                    Window.Focus();
                    interaction.SetOutput(dataContext.Output);
                });

            Interactions.ConnectToMySql.RegisterHandler(
                async interaction =>
                {
                    var dataContext = new ConnectToMySqlWindowViewModel(interaction.Input, _appData);
                    var dialog = new ConnectToMySqlWindow() { DataContext = dataContext };
                    await dialog.ShowDialog(Window);
                    Window.Focus();
                    interaction.SetOutput(dataContext.Output);
                });

            Interactions.ConnectToSqlite.RegisterHandler(
                async interaction =>
                {
                    var dataContext = new ConnectToSqliteWindowViewModel(interaction.Input, _appData);
                    var dialog = new ConnectToSqliteWindow() { DataContext = dataContext };
                    await dialog.ShowDialog(Window);
                    Window.Focus();
                    interaction.SetOutput(dataContext.Output);
                });

            Interactions.ConnectToElasticSearch.RegisterHandler(
                async interaction =>
                {
                    var dataContext = new ConnectToElasticSearchViewModel(interaction.Input, _appData);
                    var dialog = new ConnectToElasticSearchWindow() { DataContext = dataContext };
                    await dialog.ShowDialog(Window);
                    Window.Focus();
                    interaction.SetOutput(dataContext.Output);
                });

            Interactions.ConnectToFile.RegisterHandler(
                async interaction =>
                {
                    var dataContext = new ConnectToFileViewModel(interaction.Input, _appData);
                    var dialog = new ConnectToFileWindow() { DataContext = dataContext };
                    await dialog.ShowDialog(Window);
                    Window.Focus();
                    interaction.SetOutput(dataContext.Output);
                });

            Interactions.Exceptions.RegisterHandler(
                interaction =>
                {
                    RxApp.MainThreadScheduler.Schedule(async () =>
                    {
                        var dialog = new ExceptionWindow()
                        {
                            DataContext = new ExceptionWindowViewModel { Exception = interaction.Input }
                        };
                        await dialog.ShowDialog(Window);
                        Window.Focus();
                    });
                    interaction.SetOutput(Unit.Default);
                });

            Interactions.NewQuery.RegisterHandler(x =>
            {
                _appData.Queries.Add(new QueryModel
                {
                    Id = Guid.NewGuid().ToString()
                });
                x.SetOutput(Unit.Default);
            });

            Interactions.CloseQuery.RegisterHandler(x =>
            {
                var selectedQuery = _appData.GetSelectedQuery();
                if (selectedQuery != null)
                    _appData.Queries.Remove(selectedQuery);
                x.SetOutput(Unit.Default);
            });

            Interactions.OpenQuery.RegisterHandler(async x =>
            {
                OpenFileDialog openDialog = new OpenFileDialog();
                openDialog.Filters.Add(new FileDialogFilter() { Name = "Text", Extensions = { "txt" } });
                openDialog.Filters.Add(new FileDialogFilter() { Name = "All files", Extensions = { } });
                string[] paths = await openDialog.ShowAsync(this.AppData.MainWindow);
                string path = paths.FirstOrDefault();
                if (path != null)
                {
                    _appData.Queries.Add(new QueryModel
                    {
                        Id = Guid.NewGuid().ToString(),
                        Path = path
                    });
                }
                x.SetOutput(Unit.Default);
            });
        }

        public IFactory Factory
        {
            get => _factory;
            set => this.RaiseAndSetIfChanged(ref _factory, value);
        }

        public IDock Layout
        {
            get => _layout;
            set => this.RaiseAndSetIfChanged(ref _layout, value);
        }

        public string CurrentView
        {
            get => _currentView;
            set => this.RaiseAndSetIfChanged(ref _currentView, value);
        }

        public AppData AppData
        {
            get => _appData;
            set => this.RaiseAndSetIfChanged(ref _appData, value);
        }
        
        public MainWindow Window { get;  set; }
    }
}
