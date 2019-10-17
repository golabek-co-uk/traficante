using System;
using Avalonia;
using Avalonia.Logging.Serilog;
using Dock.Model;
using Traficante.Studio.Models;
using Traficante.Studio.ViewModels;
using Traficante.Studio.Views;

namespace Traficante.Studio
{
    class Program
    {
        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.
        public static void Main(string[] args) => BuildAvaloniaApp().Start(AppMain, args);

        // Avalonia configuration, don't remove; also used by visual designer.
        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .LogToDebug()
                .UseReactiveUI();

        // Your application's entry point. Here you can initialize your MVVM framework, DI
        // container, etc.
        private static void AppMain(Application app, string[] args)
        {
            var modelData = new AppData();
            var factory = new MainDockFactory(modelData);
            var layout = factory.CreateLayout();
            factory.InitLayout(layout);

            var mainWindow = new MainWindow();
            mainWindow.DataContext = new MainWindowViewModel()
            {
                Factory = factory,
                Layout = layout,
                Window = mainWindow,
            };
            modelData.MainWindow = mainWindow;
            app.Run(mainWindow);

            if (layout is IDock dock)
            {
                dock.Close();
            }
        }
    }
}
