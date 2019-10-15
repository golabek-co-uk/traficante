using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Traficante.Studio.Views
{
    public class QueryWindowView : UserControl
    {
        public QueryWindowView()
        {
            this.InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
