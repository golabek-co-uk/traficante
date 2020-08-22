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
    public class ConnectToFileViewModel : ViewModelBase
    {
        public AppData AppData { get; set; }
        public Window Window { get; set; }
        public ReactiveCommand<Unit, Unit> ConnectCommand { get; }
        public ReactiveCommand<Unit, Unit> CancelCommand { get; }
        public ReactiveCommand<Unit, Unit> DatabaseFileSelectorCommand { get; }
        public Interaction<Unit, Unit> CloseInteraction { get; } = new Interaction<Unit, Unit>();

        [Reactive]
        public FilesObjectModel Input { get; set; }

        [Reactive]
        public FilesObjectModel InputOrginal { get; set; }

        [Reactive]
        public FilesObjectModel Output { get; set; }

        [Reactive]
        public string Errors { get; set; }

        private readonly ObservableAsPropertyHelper<bool> _isConnecting;
        public bool IsConnecting => _isConnecting.Value;


        public ConnectToFileViewModel(FilesObjectModel input, AppData appData)
        {
            InputOrginal = input;
            Input = input != null ? new AppDataSerializer().Clone(input) : new FilesObjectModel();
            if (Input.ConnectionInfo.Files.Any() == false)
                Input.ConnectionInfo.Files.Add(new FileConnectionModel());
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
            if (string.IsNullOrWhiteSpace(Input.ConnectionInfo.Files[0].Path))
                errors.AppendLine("File is required.");
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
            openDialog.Title = "Choose File";
            openDialog.Filters.Add(new FileDialogFilter() { Name = "All files", Extensions = { "*" } });
            openDialog.Filters.Add(new FileDialogFilter() { Name = "CSV files", Extensions = { "csv" } });
            openDialog.Filters.Add(new FileDialogFilter() { Name = "Excel files", Extensions = { "xls", "xlsb", "xlsm", "xlsx" } });

            var path = (await openDialog.ShowAsync(Window))?.FirstOrDefault();
            if (path != null)
            {
                try
                {
                    Input.ConnectionInfo.Files[0].Path = path;
                //    Input.ConnectionInfo.Files[0].Name = new FileHelper().GetName(path);
                //    Input.ConnectionInfo.Files[0].Type = new FileHelper().GetType(path);
                }
                catch (Exception ex) { Interactions.Exceptions.Handle(ex).Subscribe(); }
            }
            return Unit.Default;
        }
    }
}
