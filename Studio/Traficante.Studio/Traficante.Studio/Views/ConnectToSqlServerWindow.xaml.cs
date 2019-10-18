using System;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using ReactiveUI;
using Traficante.Studio.Models;
using Traficante.Studio.Services;
using Traficante.Studio.ViewModels;

namespace Traficante.Studio.Views
{
    public class ConnectToSqlServerWindow : ReactiveWindow<ConnectToSqlServerWindowViewModel>
    {
        
        public Window Window => this.FindControl<Window>("Window");
        public Button Connect => this.FindControl<Button>("Connect");
        public Button Cancel => this.FindControl<Button>("Cancel");
        public TextBox ServerName => this.FindControl<TextBox>("ServerName");
        public ComboBox Authentication => this.FindControl<ComboBox>("Authentication");
        public TextBox UserId => this.FindControl<TextBox>("UserId");
        public TextBox Password => this.FindControl<TextBox>("Password");

        public ConnectToSqlServerWindow()
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

                this.Bind(ViewModel, x => x.ConnectionString.Server, x => x.ServerName.Text)
                    .DisposeWith(disposables);
                this.OneWayBind(ViewModel, x => x.CanChangeServerAndAuthentication, x => x.ServerName.IsEnabled)
                    .DisposeWith(disposables);

                this.Bind(ViewModel, x => x.ConnectionString.Authentication, x => x.Authentication.SelectedIndex,
                    x => (int)x,
                    x => (SqlServerAuthentication)x)
                .DisposeWith(disposables);
                this.OneWayBind(ViewModel, x => x.CanChangeServerAndAuthentication, x => x.Authentication.IsEnabled)
                    .DisposeWith(disposables);

                this.Bind(ViewModel, x => x.ConnectionString.UserId, x => x.UserId.Text)
                    .DisposeWith(disposables);
                this.OneWayBind(ViewModel, x => x.CanChangeUserIdAndPassword, x => x.UserId.IsEnabled)
                    .DisposeWith(disposables);

                this.Bind(ViewModel, x => x.ConnectionString.Password, x => x.Password.Text)
                    .DisposeWith(disposables);
                this.OneWayBind(ViewModel, x => x.CanChangeUserIdAndPassword, x => x.Password.IsEnabled)
                    .DisposeWith(disposables);
            });

            AvaloniaXamlLoader.Load(this);
        }
    }
}
