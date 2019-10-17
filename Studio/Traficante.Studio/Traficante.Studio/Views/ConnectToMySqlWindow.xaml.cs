using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using ReactiveUI;
using Traficante.Studio.ViewModels;

namespace Traficante.Studio.Views
{
    public class ConnectToMySqlWindow : ReactiveWindow<ConnectToMySqlWindowViewModel>
    {
        public ConnectToMySqlWindow()
        {
            this.InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
        }

        private void InitializeComponent()
        {
            this.WhenActivated(disposables =>
            {
                
            });

            AvaloniaXamlLoader.Load(this);
        }
    }
}
