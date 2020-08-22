using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;

namespace Traficante.Studio.Views
{
    public class Icons : UserControl
    {
        public Icons()
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

        private static Icons IconsInstance = new Icons();
        public static Drawing None => IconsInstance.GetIcon("None");
        public static Drawing Database => IconsInstance.GetIcon("Database");
        public static Drawing Folder => IconsInstance.GetIcon("Folder");
        public static Drawing File => IconsInstance.GetIcon("File");
        public static Drawing Table => IconsInstance.GetIcon("Table");
        public static Drawing Field => IconsInstance.GetIcon("Field");
        

    }
}
