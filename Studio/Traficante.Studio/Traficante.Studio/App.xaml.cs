using Avalonia;
using Avalonia.Markup.Xaml;

namespace Traficante.Studio
{
    public class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
