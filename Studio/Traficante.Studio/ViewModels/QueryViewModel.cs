using Dock.Model.Controls;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Text;
using Traficante.Studio.Models;
using Traficante.Studio.Services;

namespace Traficante.Studio.ViewModels
{
    public class QueryViewModel : DocumentTab
    {
        public ObservableCollection<ObjectModel> Objects => ((AppData)this.Context).Objects;

        private ObservableCollection<ObjectModel> _results;
        public ObservableCollection<ObjectModel> Results
        {
            get => _results;
            set => this.RaiseAndSetIfChanged(ref _results, value);
        }

        private ObjectModel _selectedObject;
        public ObjectModel SelectedObject
        {
            get => _selectedObject;
            set => this.RaiseAndSetIfChanged(ref _selectedObject, value);
        }

        private string _text;
        public string Text
        {
            get => _text;
            set => this.RaiseAndSetIfChanged(ref _text, value);
        }

        public ReactiveCommand<Unit, Unit> RunCommand { get; }

        public QueryViewModel()
        {
            RunCommand = ReactiveCommand.Create<Unit, Unit>(Run);
        }

        private Unit Run(Unit arg)
        {
            if (SelectedObject == null)
            {
                return Unit.Default;
            }
            if (SelectedObject is SqlServerObjectModel)
            {
                SqlServerObjectModel sqlServer = (SqlServerObjectModel)SelectedObject;
                new SqlServerService().Run(sqlServer.ConnectionInfo, Text);
            }
            return Unit.Default;
        }
    }
}
