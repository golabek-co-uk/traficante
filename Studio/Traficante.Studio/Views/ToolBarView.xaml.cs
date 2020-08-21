using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using ReactiveUI;
using System.Reactive.Disposables;
using Traficante.Studio.ViewModels;

namespace Traficante.Studio.Views
{
    public class ToolBarView : ReactiveUserControl<ToolBarViewModel>
    {
        public MenuItem ConnectToSqlServer => this.FindControl<MenuItem>("ConnectToSqlServer");
        public MenuItem ConnectToMySql => this.FindControl<MenuItem>("ConnectToMySql");
        public MenuItem ConnectToSqlite => this.FindControl<MenuItem>("ConnectToSqlite");
        public MenuItem ConnectToElasticSearch => this.FindControl<MenuItem>("ConnectToElasticSearch");
        public MenuItem ConnectToFile => this.FindControl<MenuItem>("ConnectToFile");
        
        public MenuItem NewQuery => this.FindControl<MenuItem>("NewQuery");


        public ToolBarView()
        {
            this.InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            this.WhenActivated(disposables =>
            {
                this.ViewModel = new ToolBarViewModel();
                this.ViewModel.AppData = ((MainWindow)this.Parent.Parent).ViewModel.AppData;
                this.BindCommand(ViewModel, x => x.ConnectToSqlServerCommand, x => x.ConnectToSqlServer)
                    .DisposeWith(disposables);
                this.BindCommand(ViewModel, x => x.ConnectToMySqlCommand, x => x.ConnectToMySql)
                    .DisposeWith(disposables);
                this.BindCommand(ViewModel, x => x.ConnectToSqliteCommand, x => x.ConnectToSqlite)
                    .DisposeWith(disposables);
                this.BindCommand(ViewModel, x => x.ConnectToElasticSearchCommand, x => x.ConnectToElasticSearch)
                    .DisposeWith(disposables);
                this.BindCommand(ViewModel, x => x.ConnectToFileCommand, x => x.ConnectToFile)
                    .DisposeWith(disposables);


                this.BindCommand(ViewModel, x => x.NewQueryCommand, x => x.NewQuery)
                    .DisposeWith(disposables);
            });
        }
    }
}
