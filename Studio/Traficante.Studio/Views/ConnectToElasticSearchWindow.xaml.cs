using System;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
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
    public class ConnectToElasticSearchWindow : ReactiveWindow<ConnectToElasticSearchViewModel>
    {
        public Window Window => this.FindControl<Window>("Window");
        public Button Connect => this.FindControl<Button>("Connect");
        public Button Cancel => this.FindControl<Button>("Cancel");
        public TextBox Alias => this.FindControl<TextBox>("Alias");
        public TextBox Server => this.FindControl<TextBox>("Server");
        public TextBox Errors => this.FindControl<TextBox>("Errors");

        public ConnectToElasticSearchWindow()
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
                this.BindCommand(ViewModel, x => x.ConnectCommand, x => x.Connect)
                    .DisposeWith(disposables);
                this.BindCommand(ViewModel, x => x.CancelCommand, x => x.Cancel)
                    .DisposeWith(disposables);

                ViewModel.CloseInteraction.RegisterHandler(x =>
                {
                    try
                    {
                        this.Window.Close();
                    }
                    catch { }
                    x.SetOutput(Unit.Default);
                });

                this.Bind(ViewModel, x => x.Input.ConnectionInfo.Alias, x => x.Alias.Text)
                    .DisposeWith(disposables);
                this.Bind(ViewModel, x => x.Input.ConnectionInfo.Server, x => x.Server.Text)
                    .DisposeWith(disposables);
                ViewModel.ConnectCommand.IsExecuting
                    .Select(isExecuting => !isExecuting)
                    .Subscribe(canChange => {
                        this.Alias.IsEnabled = canChange;
                        this.Server.IsEnabled = canChange;
                    })
                    .DisposeWith(disposables);

                this.Bind(ViewModel, x => x.Errors, x => x.Errors.Text);
            });

            AvaloniaXamlLoader.Load(this);
        }
    }
}
