using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Dock.Model;
using System;
using System.Collections.ObjectModel;
using System.IO;
using Traficante.Studio.ViewModels;
using Traficante.Studio.Views;

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
