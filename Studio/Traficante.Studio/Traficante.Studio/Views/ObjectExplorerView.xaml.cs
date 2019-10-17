using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using ReactiveUI;
using System.Collections.Generic;
using System.Reactive.Disposables;
using Traficante.Studio.ViewModels;

namespace Traficante.Studio.Views
{
    public class ObjectExplorerView : ReactiveUserControl<ObjectExplorerViewModel>
    {
        public MenuItem ConnectToSqlServer => this.FindControl<MenuItem>("ConnectToSqlServer");
        public MenuItem ConnectToMySql => this.FindControl<MenuItem>("ConnectToMySql");
        public TreeView Objects => this.FindControl<TreeView>("Objects");
        
        public ObjectExplorerView()
        {
            this.InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.WhenActivated(disposables => 
            {
                this.BindCommand(ViewModel, x => x.ConnectToSqlServerCommand, x => x.ConnectToSqlServer)
                    .DisposeWith(disposables);
                
                this.OneWayBind(ViewModel, x => x.Objects, x => x.Objects.Items, x => (System.Collections. IEnumerable)x)
                    .DisposeWith(disposables);
            });
            AvaloniaXamlLoader.Load(this);
        }
    }
}
