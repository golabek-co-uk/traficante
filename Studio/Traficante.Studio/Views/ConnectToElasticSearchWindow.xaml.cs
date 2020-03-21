using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using ReactiveUI;
using System.Reactive;
using System.Reactive.Disposables;
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
                this.OneWayBind(ViewModel, x => x.CanChangeControls, x => x.Server.IsEnabled)
                    .DisposeWith(disposables);
            });

            AvaloniaXamlLoader.Load(this);
        }
    }
}
