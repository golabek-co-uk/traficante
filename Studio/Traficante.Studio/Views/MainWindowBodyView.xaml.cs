using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using ReactiveUI;
using Traficante.Studio.ViewModels;

namespace Traficante.Studio.Views
{
    public class MainWindowBodyView : ReactiveUserControl<MainWindowBodyViewModel>
    {
        public MainWindowBodyView()
        {
            this.InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            this.WhenActivated(disposables =>
            {

            });
        }
    }
}
