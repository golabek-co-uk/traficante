using ReactiveUI;
using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Traficante.Connect.Connectors;
using Traficante.Studio.Models;

namespace Traficante.Studio.ViewModels
{
    public class ConnectToSqliteWindowViewModel : ViewModelBase
    {
        public ReactiveCommand<Unit, Unit> ConnectCommand { get; }
        public ReactiveCommand<Unit, Unit> CancelCommand { get; }
        public Interaction<Unit, Unit> CloseInteraction { get; } = new Interaction<Unit, Unit>();

        private SqliteConnectionModel _input;
        public SqliteConnectionModel Input
        {
            get => _input;
            set => this.RaiseAndSetIfChanged(ref _input, value);
        }

        public SqliteConnectionModel Output { get; set; }

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

        public ConnectToSqliteWindowViewModel(SqliteConnectionModel input, AppData appData)
        {
            Input = input ?? new SqliteConnectionModel();
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
                await new SqliteConnector(Input.ToConectorConfig()).TryConnectAsync(ct);
                Output = Input;
                AppData.Objects.Add(new SqliteObjectModel(Output));
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
