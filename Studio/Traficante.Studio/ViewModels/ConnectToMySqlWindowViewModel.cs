using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Traficante.Studio.Models;
using Traficante.Studio.Services;

namespace Traficante.Studio.ViewModels
{
    public class ConnectToMySqlWindowViewModel : ViewModelBase
    {
        public ReactiveCommand<Unit, Unit> ConnectCommand { get; }
        public ReactiveCommand<Unit, Unit> CancelCommand { get; }
        public Interaction<Unit, Unit> CloseInteraction { get; } = new Interaction<Unit, Unit>();

        private MySqlConnectionModel _input;
        public MySqlConnectionModel Input
        {
            get => _input;
            set => this.RaiseAndSetIfChanged(ref _input, value);
        }

        public MySqlConnectionModel Output { get; set; }

        private string _connectError;
        public string ConnectError
        {
            get => _connectError;
            set => this.RaiseAndSetIfChanged(ref _connectError, value);
        }

        private readonly ObservableAsPropertyHelper<bool> _isConnecting;
        public bool IsConnecting => _isConnecting.Value;

        private readonly ObservableAsPropertyHelper<bool> _canChangeControls;
        public bool CanChangeControls => _canChangeControls.Value;


        public AppData AppData { get; set; }

        public ConnectToMySqlWindowViewModel(MySqlConnectionModel input, AppData appData)
        {
            Input = input ?? new MySqlConnectionModel();
            AppData = appData;

            ConnectCommand = ReactiveCommand
                .CreateFromObservable(() =>
                   Observable
                       .StartAsync(ct => Connect(ct))
                       .TakeUntil(this.CancelCommand));

            CancelCommand = ReactiveCommand
                .CreateFromObservable(() =>
                   Observable
                       .StartAsync(ct => Cancel()));

            ConnectCommand.IsExecuting
                .ToProperty(this, x => x.IsConnecting, out _isConnecting);

            ConnectCommand.IsExecuting
                .Select(x => x == false)
                .ToProperty(this, x => x.CanChangeControls, out _canChangeControls);

        }

        private async Task<Unit> Cancel()
        {
            if (IsConnecting == false)
                await CloseInteraction.Handle(Unit.Default);
            return Unit.Default;
        }

        private async Task<Unit> Connect(CancellationToken ct)
        {
            try
            {
                ConnectError = string.Empty;
                await new Traficante.Connect.Connectors.MySqlConnector(Input.ToConectorConfig()).TryConnectAsync(ct);
                Output = Input;
                AppData.Objects.Add(new MySqlObjectModel(Output));
                await CloseInteraction.Handle(Unit.Default);
            }
            catch (Exception ex)
            {
                ConnectError = ex.Message;
            }
            return Unit.Default;
        }
    }
}
