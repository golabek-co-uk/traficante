using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Traficante.Studio.ViewModels;

namespace Traficante.Studio.Views
{
    public class MainWindowMenuView : ReactiveUserControl<MainWindowMenuViewModel>
    {
        public MainWindowMenuView()
        {
            this.InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            
        }
    }
}
