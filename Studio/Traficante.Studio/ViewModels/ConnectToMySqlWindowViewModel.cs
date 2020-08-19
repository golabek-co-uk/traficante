using Avalonia.Controls;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Traficante.Connect.Connectors;
using Traficante.Studio.Models;
using Traficante.Studio.Services;

namespace Traficante.Studio.ViewModels
{
    public class ConnectToMySqlWindowViewModel : ViewModelBase
    {
        public AppData AppData { get; set; }
        public ReactiveCommand<Unit, Unit> ConnectCommand { get; }
        public ReactiveCommand<Unit, Unit> CancelCommand { get; }
        public Interaction<Unit, Unit> CloseInteraction { get; } = new Interaction<Unit, Unit>();

        [Reactive]
        public MySqlObjectModel Input { get; set; }

        [Reactive]
        public MySqlObjectModel InputOrginal { get; set; }

        [Reactive]
        public MySqlObjectModel Output { get; set; }

        [Reactive]
        public string Errors { get; set; }

        private readonly ObservableAsPropertyHelper<bool> _isConnecting;
        public bool IsConnecting => _isConnecting.Value;
 

        public ConnectToMySqlWindowViewModel(MySqlObjectModel input, AppData appData)
        {
            InputOrginal = input;
            Input = input != null ? new AppDataSerializer().Clone(input) : new MySqlObjectModel();
            AppData = appData;

            ConnectCommand = ReactiveCommand
                .CreateFromObservable(() =>
                   Observable
                       .StartAsync(ct => Connect(ct))
                       .TakeUntil(this.CancelCommand));
            ConnectCommand.IsExecuting
                .ToProperty(this, x => x.IsConnecting, out _isConnecting);

            CancelCommand = ReactiveCommand
                .CreateFromObservable(() =>
                   Observable
                       .StartAsync(ct => Cancel()));
        }

        private async Task<Unit> Cancel()
        {
            if (IsConnecting == false)
                await CloseInteraction.Handle(Unit.Default);
            return Unit.Default;
        }

        public (bool IsValid, string Errors) Validate()
        {
            StringBuilder errors = new StringBuilder();
            if (string.IsNullOrWhiteSpace(Input.ConnectionInfo.Alias))
                errors.AppendLine("Alias is required.");
            if (string.IsNullOrWhiteSpace(Input.ConnectionInfo.Server))
                errors.AppendLine("Server is required.");
            if (string.IsNullOrWhiteSpace(Input.ConnectionInfo.UserId))
                errors.AppendLine("UserId is required.");
            if (string.IsNullOrWhiteSpace(Input.ConnectionInfo.Password))
                errors.AppendLine("Password is required.");
            return (errors.Length == 0, errors.ToString());
        }

        private async Task<Unit> Connect(CancellationToken ct)
        {
            Errors = string.Empty;
            try
            {
                var isValid = Validate();
                if (isValid.IsValid == false)
                {
                    Errors = isValid.Errors;
                    return Unit.Default;
                }

                await new MySqlConnector(Input.ConnectionInfo.ToConectorConfig()).TryConnect(ct);

                if (InputOrginal != null)
                    AppData.UpdateObject(InputOrginal, Input);
                else
                    AppData.AddObject(Input);

                Output = Input;
                await CloseInteraction.Handle(Unit.Default);
            }
            catch (TaskCanceledException)
            {
            }
            catch (Exception ex)
            {
                Errors = ex.Message;
            }
            return Unit.Default;
        }
    }
}
