﻿using Avalonia.Controls;
using Avalonia.Data;
using Dock.Model.Controls;
using DynamicData;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Traficante.Connect;
using Traficante.Connect.Connectors;
using Traficante.Studio.Models;
using Traficante.TSQL.Evaluator.Visitors;

namespace Traficante.Studio.ViewModels
{
    public class QueryViewModel : Document
    {
        public AppData AppData => ((AppData)this.Context);
        public QueryModel Query { get; set; }

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

        public QueryViewModel(QueryModel query, AppData context)
        {
            Context = context;
            Query = query;
            InitQuery();
            RunCommand = ReactiveCommand.CreateFromTask<Unit, Unit>(Run);
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
                    }

                    var items = connectEngine.RunAndReturnEnumerable(this.Query.Text);
                    var itemsType = items.GetType().GenericTypeArguments.FirstOrDefault();

                    itemsType
                           .GetFields()
                           .ToObservable()
                           .ObserveOn(RxApp.MainThreadScheduler)
                           .Select(x => new DataGridTextColumn
                           {
                               Header = x.Name,
                               Binding = new Binding(x.Name)
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
                        .Buffer(TimeSpan.FromSeconds(2))
                        .Subscribe(x =>
                        {
                            sourceList.AddRange(x);
                        });

                }
                catch (Exception ex)
                {
                    RxApp.MainThreadScheduler.Schedule(() =>
                    {
                        ResultsError = ex.Message;
                    });

                    return Unit.Default;
                }

                return Unit.Default;
            });
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
                    }  catch(Exception ex) {Interactions.Exceptions.Handle(ex).Subscribe();}
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
}
