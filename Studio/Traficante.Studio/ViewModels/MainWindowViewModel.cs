using System;
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

            Interactions.Exceptions.RegisterHandler(
                async interaction =>
                {
                    var dialog = new ExceptionWindow()
                    {
                        DataContext = new ExceptionWindowViewModel { Exception = interaction.Input }
                    };
                    await dialog.ShowDialog(Window);
                    Window.Focus();
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
