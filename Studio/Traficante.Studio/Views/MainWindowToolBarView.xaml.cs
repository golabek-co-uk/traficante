using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using ReactiveUI;
using System.Reactive.Disposables;
using Traficante.Studio.ViewModels;

namespace Traficante.Studio.Views
{
    public class MainWindowToolBarView : ReactiveUserControl<MainWindowToolBarViewModel>
    {
        public MenuItem ConnectToSqlServer => this.FindControl<MenuItem>("ConnectToSqlServer");
        public MenuItem ConnectToMySql => this.FindControl<MenuItem>("ConnectToMySql");
        public MenuItem NewQuery => this.FindControl<MenuItem>("NewQuery");


        public MainWindowToolBarView()
        {
            this.InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            this.WhenActivated(disposables =>
            {
                var mainView = (MainWindowBodyView)this.Parent.Parent;
                this.ViewModel = new MainWindowToolBarViewModel();
                if (mainView.ViewModel != null)
                {
                    this.ViewModel.Context = mainView.ViewModel.Context;
                    this.BindCommand(ViewModel, x => x.ConnectToSqlServerCommand, x => x.ConnectToSqlServer)
                        .DisposeWith(disposables);
                    this.BindCommand(ViewModel, x => x.ConnectToMySqlCommand, x => x.ConnectToMySql)
                        .DisposeWith(disposables);
                    this.BindCommand(ViewModel, x => x.NewQueryCommand, x => x.NewQuery)
                        .DisposeWith(disposables);
                }
            });
        }
    }
}
