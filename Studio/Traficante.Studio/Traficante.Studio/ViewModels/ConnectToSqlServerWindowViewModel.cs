using Avalonia.Controls;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Traficante.Studio.Services;

namespace Traficante.Studio.ViewModels
{
    public class ConnectToSqlServerWindowViewModel : ViewModelBase
    {
        public SqlServerConnectionString SqlServerConnectionString { get; set; } = new SqlServerConnectionString();
        public ReactiveCommand<Unit, Unit> ConnectCommand { get; }
        public ReactiveCommand<Window, Unit> CancelCommand { get; }

        private string _connectError;
        public string ConnectError
        {
            get => _connectError;
            set => this.RaiseAndSetIfChanged(ref _connectError, value);
        }

        private readonly ObservableAsPropertyHelper<bool> _isConnecting;
        public bool IsConnecting  => _isConnecting.Value;

        public ConnectToSqlServerWindowViewModel()
        {
            ConnectCommand = ReactiveCommand
                .CreateFromObservable(
                    () =>  Observable
                        .StartAsync(ct => Connect(ct))
                        .TakeUntil(this.CancelCommand));

            CancelCommand = ReactiveCommand
                .Create<Window, Unit>(
                    (w) => Cancel(w));

            ConnectCommand.IsExecuting
                .ToProperty(this, x => x.IsConnecting, out _isConnecting);
        }

        private Unit Cancel(Window window)
        {
            if (IsConnecting == false)
                window.Close();
            return Unit.Default;
        }

        private async Task<Unit> Connect(CancellationToken ct)
        {
            try
            {
                ConnectError = string.Empty;
                await new SqlServerService().TryConnect(SqlServerConnectionString, ct);
            } catch(Exception ex)
            {
                ConnectError = ex.Message;
            }
            return Unit.Default;
        }
    }

}
