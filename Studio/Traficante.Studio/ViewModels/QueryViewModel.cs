using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Data.Core;
using CsvHelper;
using Dock.Model.Controls;
using DynamicData;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reflection;
using System.Threading.Tasks;
using Traficante.Connect;
using Traficante.Connect.Connectors;
using Traficante.Studio.Models;
using Traficante.TSQL;
using Traficante.TSQL.Evaluator.Visitors;

namespace Traficante.Studio.ViewModels
{
    public class QueryViewModel : Document
    {
        public AppData AppData => ((AppData)this.Context);
        public QueryModel Query { get; set; }

        private string _selectedText;
        public string SelectedText
        {
            get => _selectedText;
            set => this.RaiseAndSetIfChanged(ref _selectedText, value);
        }

        private string _resultsError;
        public string ResultsError
        {
            get => _resultsError;
            set => this.RaiseAndSetIfChanged(ref _resultsError, value);
        }

        private string _resultsMessage;
        public string ResultsMessage
        {
            get => _resultsError;
            set => this.RaiseAndSetIfChanged(ref _resultsMessage, value);
        }

        private bool _resultsAreVisible = false;
        public bool ResultsAreVisible
        {
            get => _resultsAreVisible;
            set => this.RaiseAndSetIfChanged(ref _resultsAreVisible, value);
        }

        private string _rowsCount;
        public string ResultsCount
        {
            get => _rowsCount;
            set => this.RaiseAndSetIfChanged(ref _rowsCount, value);
        }

        private ReadOnlyObservableCollection<object> _resultsData;
        public ReadOnlyObservableCollection<object> ResultsData
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
        public ReactiveCommand<Unit, Unit> SaveResultsAsCommand { get; set; }
        

        public QueryViewModel(QueryModel query, AppData context)
        {
            Context = context;
            Query = query;
            InitQuery();
            RunCommand = ReactiveCommand.CreateFromTask<Unit, Unit>(Run);
            SaveResultsAsCommand = ReactiveCommand.CreateFromTask<Unit, Unit>(SaveResultsAs);
            Interactions.SaveQuery.RegisterHandler(Save);
            Interactions.SaveAsQuery.RegisterHandler(SaveAs);
        }

        private void InitQuery()
        {
            // Load text
            Id = this.Query.Id;

            // Adding star to the title when the query is dirty (was changed)
            this
                .WhenAnyValue(x => x.Query.Path, x => x.Query.IsDirty)
                .Subscribe(x =>
                {
                    if (x.Item1 != null)
                        if (x.Item2)
                            Title = System.IO.Path.GetFileName(x.Item1) + "*";
                        else
                            Title = System.IO.Path.GetFileName(x.Item1);
                    else
                        if (x.Item2)
                        Title = "New Query*";
                    else
                        Title = "New Query";
                });
        }

        public Task<Unit> Run(Unit arg)
        {

            return Task.Run(() =>
            {
                RxApp.MainThreadScheduler.Schedule(() =>
                {
                    ResultsMessage = string.Empty;
                    ResultsError = string.Empty;
                    ResultsCount = string.Empty;
                    this.ResultsAreVisible = true;
                    this.ResultsData = new ReadOnlyObservableCollection<object>(new ObservableCollection<object>());
                    this.ResultsDataColumns.Clear();
                });

                try
                {
                    ConnectEngine connectEngine = new ConnectEngine();
                    connectEngine.AddConector(new CsvConnectorConfig { Alias = "csv" });
                    foreach (var obj in this.AppData.Objects)
                    {
                        if (obj is SqlServerObjectModel)
                            connectEngine.AddConector(((SqlServerObjectModel)obj).ConnectionInfo.ToConectorConfig());
                        if (obj is MySqlObjectModel)
                            connectEngine.AddConector(((MySqlObjectModel)obj).ConnectionInfo.ToConectorConfig());
                        if (obj is SqliteObjectModel)
                            connectEngine.AddConector(((SqliteObjectModel)obj).ConnectionInfo.ToConectorConfig());
                        if (obj is ElasticSearchObjectModel)
                            connectEngine.AddConector(((ElasticSearchObjectModel)obj).ConnectionInfo.ToConectorConfig());
                    }

                    var sql = string.IsNullOrEmpty(this.SelectedText) == false ? this.SelectedText : this.Query.Text;
                    var items = connectEngine.RunAndReturnEnumerable(sql);
                    var itemsType = items.GetType().GenericTypeArguments.FirstOrDefault();

                    itemsType
                           .GetFields()
                           .ToObservable()
                           .ObserveOn(RxApp.MainThreadScheduler)
                           .Select(x => new DataGridTextColumn
                           {
                               Header = x.Name,
                               Binding = new DataBinding(x.Name),
                           })
                           .Subscribe(this.ResultsDataColumns.Add);


                    Type itemWrapperType = new ExpressionHelper().CreateWrapperTypeFor(itemsType); ;
                    FieldInfo itemWrapperInnerField = itemWrapperType.GetFields().FirstOrDefault(x => x.Name == "_inner"); ;
          
                    ReadOnlyObservableCollection<object> data;
                    var sourceList = new SourceList<object>();
                    sourceList
                        .Connect()
                        .Transform(item =>
                        {
                            var itemWrapper = Activator.CreateInstance(itemWrapperType);
                            itemWrapperInnerField.SetValue(itemWrapper, item);
                            return itemWrapper;
                        })
                        .ObserveOn(RxApp.MainThreadScheduler)
                        .Bind(out data)
                        .DisposeMany()
                        .Subscribe(x =>
                        {
                            this.ResultsData = data;

                        });


                    ((IEnumerable<object>)items)
                        .ToObservable()
                        .Buffer(TimeSpan.FromSeconds(1))
                        .Subscribe(x =>
                        {
                            sourceList.AddRange(x);
                            
                            RxApp.MainThreadScheduler.Schedule(() =>
                            {
                                ResultsCount = sourceList.Count + " rows";
                            });
                        });

                }
                catch (TSQLException ex)
                {
                    RxApp.MainThreadScheduler.Schedule(() =>
                    {
                        string error = $"Error: {ex.Message}";
                        if (ex.LineNumber.HasValue)
                            error += $"\r\nLine number: {ex.LineNumber}";
                        if (ex.ColumnNumber.HasValue)
                            error += $"\r\nColumn number: {ex.ColumnNumber}";
                        if (ex.ColumnNumber.HasValue)
                            error += $"\r\n\r\nStack trace: {ex.StackTrace}";
                        ResultsError = error;
                    });
                    return Unit.Default;
                }
                catch (Exception ex)
                {
                    RxApp.MainThreadScheduler.Schedule(() =>
                    {
                        ResultsError = ex.ToString();
                    });
                    return Unit.Default;
                }

                return Unit.Default;
            });
        }

        public async Task<Unit> SaveResultsAs(Unit arg)
        {
            SaveFileDialog saveDialog = new SaveFileDialog();
            saveDialog.Filters.Add(new FileDialogFilter() { Name = "CSV (comma delimited)", Extensions = { "csv" } });
            var path = await saveDialog.ShowAsync(this.AppData.MainWindow);
            if (path != null)
            {
                try
                {
                    using (var writer = new StreamWriter(path))
                    using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                    {
                        csv.WriteRecords(this.ResultsData);
                    }
                }
                catch (Exception ex) { Interactions.Exceptions.Handle(ex).Subscribe(); }
            }
            return Unit.Default;
        }

        private async Task Save(InteractionContext<Unit, Unit> context)
        {
            var selectedQuery = AppData.GetSelectedQuery();
            if (selectedQuery == this.Query)
            {
                context.SetOutput(Unit.Default);
                try { File.WriteAllText(this.Query.AutoSavePath, this.Query.Text); } catch { }
                if (Query.Path != null)
                {
                    try
                    {
                        System.IO.File.WriteAllText(this.Query.Path, this.Query.Text);
                        this.Query.IsDirty = false;
                    } catch (Exception ex) { Interactions.Exceptions.Handle(ex).Subscribe(); }
                }
                else
                {
                    SaveFileDialog saveDialog = new SaveFileDialog();
                    saveDialog.Filters.Add(new FileDialogFilter() { Name = "Text", Extensions = { "txt" } });
                    saveDialog.Filters.Add(new FileDialogFilter() { Name = "All files", Extensions = { } });
                    var path = await saveDialog.ShowAsync(this.AppData.MainWindow);
                    if (path != null)
                    {
                        try
                        {
                            System.IO.File.WriteAllText(path, this.Query.Text);
                            this.Query.Path = path;
                            this.Query.IsDirty = false;
                        } catch (Exception ex) { Interactions.Exceptions.Handle(ex).Subscribe(); }
                    }
                }
            }
        }

        private async Task SaveAs(InteractionContext<Unit, Unit> context)
        {
            var selectedQuery = AppData.GetSelectedQuery();
            if (selectedQuery == this.Query)
            {
                context.SetOutput(Unit.Default);
                try { File.WriteAllText(this.Query.AutoSavePath, this.Query.Text); } catch { }
                SaveFileDialog saveDialog = new SaveFileDialog();
                saveDialog.Filters.Add(new FileDialogFilter() { Name = "Text", Extensions = { "txt" } });
                saveDialog.Filters.Add(new FileDialogFilter() { Name = "All files", Extensions = { } });
                var path = await saveDialog.ShowAsync(this.AppData.MainWindow);
                if (path != null)
                {
                    try
                    {
                        System.IO.File.WriteAllText(path, Query.Text);
                        this.Query.Path = path;
                        this.Query.IsDirty = false;
                    }
                    catch (Exception ex) { Interactions.Exceptions.Handle(ex).Subscribe(); }
                }
            }
        }
    }

    public class DataBinding: IBinding
    {
        public string Path { get; }

        public DataBinding(string path)
        {
            Path = path;
        }
        
        public InstancedBinding Initiate(IAvaloniaObject target, AvaloniaProperty targetProperty, object anchor = null, bool enableDataValidation = false)
        {
            target
                .GetObservable(StyledElement.DataContextProperty)
                .Subscribe(x =>
                {
                    if (x != null)
                    {
                        var type = x.GetType();
                        var field = type.GetProperty(this.Path);
                        var value = field.GetValue(x);
                        target.SetValue(targetProperty, value?.ToString() ?? "", BindingPriority.LocalValue);
                    }
                    else
                    {
                        target.SetValue(targetProperty, "", BindingPriority.LocalValue);

                    }
                });

            return null;
        }
    }

    //public class ValueSubject : ISubject<object>, IDisposable
    //{
    //    private IAvaloniaObject target;
    //    private AvaloniaProperty targetProperty;

    //    public ValueSubject(IAvaloniaObject target, AvaloniaProperty targetProperty)
    //    {
    //        this.target = target;
    //        this.targetProperty = targetProperty;
    //    }

    //    public void Dispose()
    //    {
            
    //    }

    //    public void OnCompleted()
    //    {
           
    //    }

    //    public void OnError(Exception error)
    //    {
            
    //    }

    //    public void OnNext(object value)
    //    {
    //        Console.WriteLine(value?.ToString());
    //    }

    //    public IDisposable Subscribe(IObserver<object> observer)
    //    {
            
    //        return this;
    //    }
    //}

}
