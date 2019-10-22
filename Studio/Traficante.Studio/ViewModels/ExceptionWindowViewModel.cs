using Avalonia.Controls;
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
    public class ExceptionWindowViewModel : ViewModelBase
    {
        public ReactiveCommand<Unit, Unit> CloseCommand { get; }
        public Interaction<Unit, Unit> CloseInteraction { get; } = new Interaction<Unit, Unit>();

        private string _message;
        public string Message
        {
            get => _message;
            set => this.RaiseAndSetIfChanged(ref _message, value);
        }

        private string _details;
        public string Details
        {
            get => _details;
            set => this.RaiseAndSetIfChanged(ref _details, value);
        }

        private Exception _exception;
        public Exception Exception
        {
            get => _exception;
            set
            {
                this.RaiseAndSetIfChanged(ref _exception, value);
                Message = value.Message;
            }
        }

        public ExceptionWindowViewModel()
        {
            CloseCommand = ReactiveCommand
                .CreateFromTask(async (x) => await Close());

        }

        private async Task<Unit> Close()
        {
            await CloseInteraction.Handle(Unit.Default);
            return Unit.Default;
        }
    }

}
