using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using ReactiveUI;
using Traficante.Studio.ViewModels;

namespace Traficante.Studio.Views
{
    public class MenuView : ReactiveUserControl<MenuViewModel>
    {
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
                this.ViewModel.AppData = ((MainWindow)this.Parent.Parent).ViewModel.AppData;
                //this.BindCommand(ViewModel, x => x.ConnectToSqlServerCommand, x => x.ConnectToSqlServer)
                //    .DisposeWith(disposables);
                //this.BindCommand(ViewModel, x => x.ConnectToMySqlCommand, x => x.ConnectToMySql)
                //    .DisposeWith(disposables);
                //this.BindCommand(ViewModel, x => x.NewQueryCommand, x => x.NewQuery)
                //    .DisposeWith(disposables);
            });
        }
    }
}
