using System;
using System.Reactive.Disposables;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using ReactiveUI;
using Traficante.Studio.Models;
using Traficante.Studio.Services;
using Traficante.Studio.ViewModels;

namespace Traficante.Studio.Views
{
    public class ExceptionWindow : ReactiveWindow<ExceptionWindowViewModel>
    {
        
        public Window Window => this.FindControl<Window>("Window");
        public Button Close => this.FindControl<Button>("Close");
        public TextBlock Message => this.FindControl<TextBlock>("Message");
        public TextBlock Details => this.FindControl<TextBlock>("Details");

        public ExceptionWindow()
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
                this.BindCommand(ViewModel, x => x.CloseCommand, x => x.Close)
                    .DisposeWith(disposables);
                ViewModel.CloseInteraction.RegisterHandler(x =>
                {
                    Window.Close();
                });

                this.Bind(ViewModel, x => x.Message, x => x.Message.Text)
                    .DisposeWith(disposables);

                this.Bind(ViewModel, x => x.Details, x => x.Details.Text)
                    .DisposeWith(disposables);
                
            });

            AvaloniaXamlLoader.Load(this);
        }
    }
}
