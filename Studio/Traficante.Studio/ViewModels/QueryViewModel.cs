using Avalonia.Controls;
using Avalonia.Data;
using Dock.Model.Controls;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Text;
using Traficante.Studio.Models;
using Traficante.Studio.Services;
using Traficante.TSQL.Evaluator.Visitors;

namespace Traficante.Studio.ViewModels
{
    public class QueryViewModel : DocumentTab
    {
        public ObservableCollection<ObjectModel> Objects => ((AppData)this.Context).Objects;

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

        private bool _resultsAreVisible;
        public bool ResultsAreVisible
        {
            get => _resultsAreVisible;
            set => this.RaiseAndSetIfChanged(ref _resultsAreVisible, value);
        }

        private ObservableCollection<object> _resultsData = new ObservableCollection<object>();
        public ObservableCollection<object> ResultsData
        {
            get => _resultsData;
            set => this.RaiseAndSetIfChanged(ref _resultsData, value);
        }

        private ObservableCollection<DataGridColumn> _resultsDataColumns = new ObservableCollection<DataGridColumn>();
        public ObservableCollection<DataGridColumn> ResultsDataColumns
        {
            get => _resultsDataColumns;
            set => this.RaiseAndSetIfChanged(ref _resultsDataColumns, value);
        }

        public ReactiveCommand<Unit, Unit> RunCommand { get; }

        public QueryViewModel()
        {
            RunCommand = ReactiveCommand.Create<Unit, Unit>(Run);
        }

        private Unit Run(Unit arg)
        {
            if (SelectedObject == null)
                return Unit.Default;
            
            if (SelectedObject is SqlServerObjectModel)
            {
                this.ResultsAreVisible = true;
                this.ResultsData.Clear();
                this.ResultsDataColumns.Clear();

                SqlServerObjectModel sqlServer = (SqlServerObjectModel)SelectedObject;

                var results = new SqlServerService().Run(sqlServer.ConnectionInfo, Text, 
                    itemType => 
                    {
                        itemType.GetFields().ToList().ForEach(x =>
                        {
                            this.ResultsDataColumns.Add(new DataGridTextColumn
                            {
                                Header = x.Name,
                                Binding = new Binding(x.Name)
                            });
                        });
                        
                    });

                Type itemType = null;
                Type itemWrapperType = null;
                foreach (var item in results)
                {
                    if (itemType == null)
                    {
                        itemType = item.GetType();
                        itemWrapperType = new ExpressionHelper().CreateWrapperTypeFor(itemType);
                    }
                    var itemWrapper = Activator.CreateInstance(itemWrapperType);
                    itemWrapperType
                        .GetFields()
                        .FirstOrDefault(x => x.Name == "_inner")
                        .SetValue(itemWrapper, item);
                    this.ResultsData.Add(itemWrapper);
                    //this.Results.Add(new TestObj { Id = 1, Name = "asdf" });
                }
            }

            if (SelectedObject is MySqlObjectModel)
            {
                this.ResultsAreVisible = true;
                this.ResultsData.Clear();
                this.ResultsDataColumns.Clear();

                MySqlObjectModel mySql = (MySqlObjectModel)SelectedObject;

                var results = new MySqlService().Run(mySql.ConnectionInfo, Text,
                    itemType =>
                    {
                        itemType.GetFields().ToList().ForEach(x =>
                        {
                            this.ResultsDataColumns.Add(new DataGridTextColumn
                            {
                                Header = x.Name,
                                Binding = new Binding(x.Name)
                            });
                        });

                    });

                Type itemType = null;
                Type itemWrapperType = null;
                foreach (var item in results)
                {
                    if (itemType == null)
                    {
                        itemType = item.GetType();
                        itemWrapperType = new ExpressionHelper().CreateWrapperTypeFor(itemType);
                    }
                    var itemWrapper = Activator.CreateInstance(itemWrapperType);
                    itemWrapperType
                        .GetFields()
                        .FirstOrDefault(x => x.Name == "_inner")
                        .SetValue(itemWrapper, item);
                    this.ResultsData.Add(itemWrapper);
                    //this.Results.Add(new TestObj { Id = 1, Name = "asdf" });
                }
            }

            return Unit.Default;
        }
    }
}
