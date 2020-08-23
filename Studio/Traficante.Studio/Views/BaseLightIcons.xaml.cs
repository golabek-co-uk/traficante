using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Styling;

namespace Traficante.Studio.Views
{
    public class BaseLightIcons : ResourceDictionary
    {

        public BaseLightIcons()
        {
            this.InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public Drawing GetIcon(string name)
        {
            return this.FindResource(name) as Drawing;
        }

        private static BaseLightIcons IconsInstance = new BaseLightIcons();
        public static Drawing None => IconsInstance.GetIcon("None");
        public static Drawing Database => IconsInstance.GetIcon("Database");
        public static Drawing Folder => IconsInstance.GetIcon("Folder");
        public static Drawing File => IconsInstance.GetIcon("File");
        public static Drawing Table => IconsInstance.GetIcon("Table");
        public static Drawing Field => IconsInstance.GetIcon("Field");
    }
}
