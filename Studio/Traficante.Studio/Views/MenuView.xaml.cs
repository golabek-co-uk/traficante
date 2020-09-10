using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using ReactiveUI;
using System.Reactive.Disposables;
using Traficante.Studio.ViewModels;

namespace Traficante.Studio.Views
{
    public class MenuView : ReactiveUserControl<MenuViewModel>
    {
        public MenuItem ConnectToSqlServer => this.FindControl<MenuItem>("ConnectToSqlServer");
        public MenuItem ConnectToMySql => this.FindControl<MenuItem>("ConnectToMySql");
        public MenuItem ConnectToSqlite => this.FindControl<MenuItem>("ConnectToSqlite");
        public MenuItem ConnectToElasticSearch => this.FindControl<MenuItem>("ConnectToElasticSearch");
        public MenuItem ConnectToFile => this.FindControl<MenuItem>("ConnectToFile");
        public MenuItem NewQuery => this.FindControl<MenuItem>("NewQuery");
        public MenuItem New => this.FindControl<MenuItem>("New");
        public MenuItem Open => this.FindControl<MenuItem>("Open");
        public MenuItem Save => this.FindControl<MenuItem>("Save");
        public MenuItem SaveAs => this.FindControl<MenuItem>("SaveAs");
        public MenuItem SaveAll => this.FindControl<MenuItem>("SaveAll");
        public MenuItem Close => this.FindControl<MenuItem>("Close");
        public MenuItem Copy => this.FindControl<MenuItem>("Copy");
        public MenuItem Paste => this.FindControl<MenuItem>("Paste");
        public MenuItem Exit => this.FindControl<MenuItem>("Exit");


        public MenuView()
        {
            this.InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            this.WhenActivated(disposables =>
            {
                this.ViewModel = new MenuViewModel();
                this.ViewModel.Context = ((MainWindow)this.Parent.Parent).ViewModel.AppData;
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
                this.BindCommand(ViewModel, x => x.NewCommand, x => x.NewQuery)
                    .DisposeWith(disposables);
                this.BindCommand(ViewModel, x => x.NewCommand, x => x.New)
                    .DisposeWith(disposables);
                this.BindCommand(ViewModel, x => x.OpenCommand, x => x.Open)
                    .DisposeWith(disposables);
                this.BindCommand(ViewModel, x => x.SaveCommand, x => x.Save)
                    .DisposeWith(disposables);
                this.BindCommand(ViewModel, x => x.SaveAsCommand, x => x.SaveAs)
                    .DisposeWith(disposables);
                this.BindCommand(ViewModel, x => x.SaveAllCommand, x => x.SaveAll)
                    .DisposeWith(disposables);
                this.BindCommand(ViewModel, x => x.CloseCommand, x => x.Close)
                    .DisposeWith(disposables);
                this.BindCommand(ViewModel, x => x.CopyCommand, x => x.Copy)
                    .DisposeWith(disposables);
                this.BindCommand(ViewModel, x => x.PasteCommand, x => x.Paste)
                    .DisposeWith(disposables);
                this.BindCommand(ViewModel, x => x.ExitCommand, x => x.Exit)
                    .DisposeWith(disposables);
            });
        }
    }
}
