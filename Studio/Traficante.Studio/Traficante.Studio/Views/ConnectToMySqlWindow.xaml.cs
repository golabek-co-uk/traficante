using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Traficante.Studio.Views
{
    public class ConnectToMySqlWindow : Window
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
            AvaloniaXamlLoader.Load(this);
        }
    }
}
