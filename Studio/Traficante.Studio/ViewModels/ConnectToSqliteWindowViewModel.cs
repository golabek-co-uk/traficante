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
    public class ConnectToSqliteWindowViewModel : ViewModelBase
    {
        public AppData AppData { get; set; }
        public Window Window { get; set; }
        public ReactiveCommand<Unit, Unit> ConnectCommand { get; }
        public ReactiveCommand<Unit, Unit> CancelCommand { get; }
        public ReactiveCommand<Unit, Unit> DatabaseFileSelectorCommand { get; }
        public Interaction<Unit, Unit> CloseInteraction { get; } = new Interaction<Unit, Unit>();

        [Reactive]
        public SqliteObjectModel Input { get; set; }

        [Reactive]
        public SqliteObjectModel InputOrginal { get; set; }

        [Reactive]
        public SqliteObjectModel Output { get; set; }

        [Reactive]
        public string Errors { get; set; }

        private readonly ObservableAsPropertyHelper<bool> _isConnecting;
        public bool IsConnecting => _isConnecting.Value;


        public ConnectToSqliteWindowViewModel(SqliteObjectModel input, AppData appData)
        {
            InputOrginal = input;
            Input = input != null ? new AppDataSerializer().Clone(input) : new SqliteObjectModel();
            AppData = appData;

            ConnectCommand = ReactiveCommand
                .CreateFromObservable(() =>
                   Observable.StartAsync(ct => Connect(ct)).TakeUntil(this.CancelCommand));
            ConnectCommand.IsExecuting
                .ToProperty(this, x => x.IsConnecting, out _isConnecting);

            CancelCommand = ReactiveCommand
                .CreateFromObservable(() => Observable.StartAsync(ct => Cancel()));

            DatabaseFileSelectorCommand = ReactiveCommand.CreateFromTask<Unit, Unit>(SelectFile);
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
            if (string.IsNullOrWhiteSpace(Input.ConnectionInfo.Database))
                errors.AppendLine("Database is required.");
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

                await new SqliteConnector(Input.ConnectionInfo.ToConectorConfig()).TryConnect(ct);
                
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

        public async Task<Unit> SelectFile(Unit unit)
        {
            OpenFileDialog openDialog = new OpenFileDialog();
            openDialog.AllowMultiple = false;
            openDialog.Title = "Choose Sqlite database";
            openDialog.Filters.Add(new FileDialogFilter() { Name = "Sqlite files", Extensions = { "db" } });
            openDialog.Filters.Add(new FileDialogFilter() { Name = "All files", Extensions = { "*" } });
            var path = await openDialog.ShowAsync(Window);
            if (path != null)
            {
                try
                {
                    Input.ConnectionInfo.Database = path.FirstOrDefault();
                }
                catch (Exception ex) { Interactions.Exceptions.Handle(ex).Subscribe(); }
            }
            return Unit.Default;
        }
    }
}
