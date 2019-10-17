using System;
using Dock.Model;
using ReactiveUI;
using Traficante.Studio.Models;
using Traficante.Studio.Views;

namespace Traficante.Studio.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private IDockFactory _factory;
        private IView _layout;
        private string _currentView;

        public MainWindowViewModel()
        {
            Interactions.ConnectToSqlServer.RegisterHandler(
                async interaction =>
                {
                    var dataContext = new ConnectToSqlServerWindowViewModel
                    {
                        ConnectionString = interaction.Input ?? new SqlServerConnectionString()
                    };
                    var dialog = new ConnectToSqlServerWindow()
                    {
                        DataContext = dataContext
                    };
                    await dialog.ShowDialog(Window);
                    Window.Focus();
                    if (dataContext.ConnectWasSuccesful)
                        interaction.SetOutput(dataContext.ConnectWasSuccesful ? dataContext.ConnectionString : null);
                });
        }

        public IDockFactory Factory
        {
            get => _factory;
            set => this.RaiseAndSetIfChanged(ref _factory, value);
        }

        public IView Layout
        {
            get => _layout;
            set => this.RaiseAndSetIfChanged(ref _layout, value);
        }

        public string CurrentView
        {
            get => _currentView;
            set => this.RaiseAndSetIfChanged(ref _currentView, value);
        }
        public MainWindow Window { get;  set; }
    }
}
