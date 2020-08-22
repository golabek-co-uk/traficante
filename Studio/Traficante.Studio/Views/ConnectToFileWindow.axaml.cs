using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using ReactiveUI;
using ReactiveUI.Validation.Extensions;
using System;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Traficante.Studio.ViewModels;

namespace Traficante.Studio.Views
{
    public class ConnectToFileWindow : ReactiveWindow<ConnectToFileViewModel>
    {
        public Window Window => this.FindControl<Window>("Window");
        public Button Connect => this.FindControl<Button>("Connect");
        public Button Cancel => this.FindControl<Button>("Cancel");
        public TextBox Alias => this.FindControl<TextBox>("Alias");
        public TextBox File => this.FindControl<TextBox>("File");
        public Button FileSelector => this.FindControl<Button>("FileSelector");
        public TextBox Errors => this.FindControl<TextBox>("Errors");

        public ConnectToFileWindow()
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
                    try { this.Close(); } catch { }
                    x.SetOutput(Unit.Default);
                });

                this.Bind(ViewModel, x => x.Input.ConnectionInfo.Alias, x => x.Alias.Text)
                    .DisposeWith(disposables);
                this.Bind(ViewModel, x => x.Input.ConnectionInfo.Files[0].Path, x => x.File.Text)
                    .DisposeWith(disposables);
                this.BindCommand(ViewModel, x => x.DatabaseFileSelectorCommand, x => x.FileSelector)
                    .DisposeWith(disposables);
                ViewModel.ConnectCommand.IsExecuting
                    .Select(isExecuting => !isExecuting)
                    .Subscribe(canChange => {
                        this.Alias.IsEnabled = canChange;
                        this.File.IsEnabled = canChange;
                        this.FileSelector.IsEnabled = canChange;
                    })
                    .DisposeWith(disposables);

                this.Bind(ViewModel, x => x.Errors, x => x.Errors.Text);

                ViewModel.Window = this;
            });

            AvaloniaXamlLoader.Load(this);
        }
    }
}
