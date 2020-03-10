using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using ReactiveUI;
using System.Collections;
using System.Collections.Generic;
using System.Reactive.Disposables;
using Traficante.Studio.ViewModels;

namespace Traficante.Studio.Views
{
    public class ObjectExplorerView : ReactiveUserControl<ObjectExplorerViewModel>
    {
        public TreeView Objects => this.FindControl<TreeView>("Objects");
        
        public ObjectExplorerView()
        {
            this.InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.WhenActivated(disposables => 
            {
                this.OneWayBind(ViewModel, x => x.Objects, x => x.Objects.Items, x => (System.Collections. IEnumerable)x)
                    .DisposeWith(disposables);
            });
            AvaloniaXamlLoader.Load(this);
        }
    }
}
