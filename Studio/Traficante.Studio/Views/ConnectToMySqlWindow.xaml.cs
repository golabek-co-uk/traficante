using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using ReactiveUI;
using System.Reactive.Disposables;
using Traficante.Studio.ViewModels;

namespace Traficante.Studio.Views
{
    public class ConnectToMySqlWindow : ReactiveWindow<ConnectToMySqlWindowViewModel>
    {
        public Window Window => this.FindControl<Window>("Window");
        public Button Connect => this.FindControl<Button>("Connect");
        public Button Cancel => this.FindControl<Button>("Cancel");
        public TextBox ServerName => this.FindControl<TextBox>("ServerName");
        public TextBox UserId => this.FindControl<TextBox>("UserId");
        public TextBox Password => this.FindControl<TextBox>("Password");

        public ConnectToMySqlWindow()
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
                });

                this.Bind(ViewModel, x => x.Input.Server, x => x.ServerName.Text)
                    .DisposeWith(disposables);
                this.OneWayBind(ViewModel, x => x.CanChangeControls, x => x.ServerName.IsEnabled)
                    .DisposeWith(disposables);


                this.Bind(ViewModel, x => x.Input.UserId, x => x.UserId.Text)
                    .DisposeWith(disposables);
                this.OneWayBind(ViewModel, x => x.CanChangeControls, x => x.UserId.IsEnabled)
                    .DisposeWith(disposables);

                this.Bind(ViewModel, x => x.Input.Password, x => x.Password.Text)
                    .DisposeWith(disposables);
                this.OneWayBind(ViewModel, x => x.CanChangeControls, x => x.Password.IsEnabled)
                    .DisposeWith(disposables);
            });

            AvaloniaXamlLoader.Load(this);
        }
    }
}
