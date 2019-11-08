using Avalonia.Controls;
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
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Traficante.Studio.Models;
using Traficante.Studio.Services;
using Traficante.TSQL.Evaluator.Visitors;

namespace Traficante.Studio.ViewModels
{
    public class QueryViewModel : DocumentTab
    {
        public AppData AppData => ((AppData)this.Context);
        public ObservableCollection<ObjectModel> Objects { get; set; }
        public QueryModel Query { get; set; }

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

        private bool _resultsAreVisible = true;
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

        public QueryViewModel(string id, string title, AppData context)
        {
            Id = id;
            Title = title;
            Context = context;
            Objects = AppData.Objects;
            Query = AppData.Queries.FirstOrDefault(x => x.Id == this.Id);
            RunCommand = ReactiveCommand.Create<Unit, Unit>((x) =>
            {
                Task.Run(() => Run(x));
                return Unit.Default;
            });

            InitSelectedObject();
            InitAutoSave();
        }

        public void InitSelectedObject()
        {
            this.SelectedObject = AppData
                .Objects
                .FirstOrDefault(x => x.Name == Query.SelectedObjectName);

            this.WhenAnyValue(x => x.SelectedObject)
                .Select(x => x?.Name)
                .Subscribe(x =>
                {
                    Query.SelectedObjectName = x;
                });
        }

        public void InitAutoSave()
        {
            var autoSavePath = Path.Combine("AutoSave", this.Id);
            try
            {
                if (Directory.Exists("AutoSave") == false)
                    Directory.CreateDirectory("AutoSave");
                this.Text = File.ReadAllText(autoSavePath);
            } 
            catch { }
            this
                .WhenAnyValue(x => x.Text)
                .DistinctUntilChanged()
                .Throttle(TimeSpan.FromSeconds(1))
                .Subscribe(x =>
                {
                    try { File.WriteAllText(autoSavePath, x); } catch { }
                });
        }

        private Unit Run(Unit arg)
        {
            if (SelectedObject == null)
                return Unit.Default;

            //ResultsMessage = string.Empty;
            //ResultsError = string.Empty;
            //this.ResultsAreVisible = true;
            //this.ResultsData.Clear();
            //this.ResultsDataColumns.Clear();

            try
            {
                if (SelectedObject is SqlServerObjectModel)
                {
                    SqlServerObjectModel sqlServer = (SqlServerObjectModel)SelectedObject;

                    var results = new SqlServerService().Run(sqlServer.ConnectionInfo, Text,
                        itemType =>
                        {
                            itemType
                                .GetFields()
                                .ToObservable()
                                .ObserveOn(RxApp.MainThreadScheduler)
                                .Select(x => new DataGridTextColumn
                                {
                                    Header = x.Name,
                                    Binding = new Binding(x.Name)
                                })
                                .Subscribe(this.ResultsDataColumns.Add);
                        });

                    //Type itemType = null;
                    //Type itemWrapperType = null;
                    //Observable
                    //    .Create<object>(obs =>
                    //    {
                    //        Observable.Start(() => {
                    //            foreach (var item in results)
                    //            {
                    //                if (itemType == null)
                    //                {
                    //                    itemType = item.GetType();
                    //                    itemWrapperType = new ExpressionHelper().CreateWrapperTypeFor(itemType);
                    //                }
                    //                var itemWrapper = Activator.CreateInstance(itemWrapperType);
                    //                itemWrapperType
                    //                    .GetFields()
                    //                    .FirstOrDefault(x => x.Name == "_inner")
                    //                    .SetValue(itemWrapper, item);
                    //                obs.OnNext(itemWrapper);
                    //            }
                    //            obs.OnCompleted();
                    //        }, RxApp.TaskpoolScheduler);

                    //        return Disposable.Empty;
                    //    })
                    //    .Buffer(100)
                    //    .Select(x =>
                    //    {
                    //        Thread.Sleep(30);
                    //        return x;
                    //    })
                    //    .ObserveOn(RxApp.MainThreadScheduler)
                    //    .Catch<object, Exception>(ex =>
                    //    {
                    //        ResultsError = ex.Message;
                    //        return Observable.Empty<object>();
                    //    })
                    //    .Subscribe(x =>
                    //    {
                    //        foreach (var item in (IList<object>)x)
                    //            this.ResultsData.Add(item);
                    //    });

                    Type itemType = null;
                    Type itemWrapperType = null;

                    //var iiiii = new SourceList<object>();
                    //iiiii
                    //    .Connect()
                    //    .Transform()


                    results
                        .ToObservable()
                        .Select(item =>
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
                            return itemWrapper;
                        })
                        .Buffer(1000)
                        .ObserveOn(RxApp.MainThreadScheduler)
                        .Catch<object, Exception>(ex =>
                        {
                            ResultsError = ex.Message;
                            return Observable.Empty<object>();
                        })
                        .Subscribe(x =>
                        {
                            foreach (var item in (IList<object>)x)
                                this.ResultsData.Add(item);
                        });
                }

                if (SelectedObject is MySqlObjectModel)
                {
                    MySqlObjectModel mySql = (MySqlObjectModel)SelectedObject;

                    var results = new MySqlService().Run(mySql.ConnectionInfo, Text,
                        itemType =>
                        {
                            itemType
                                .GetFields()
                                .ToObservable()
                                .Select(x => new DataGridTextColumn
                                {
                                    Header = x.Name,
                                    Binding = new Binding(x.Name)
                                })
                                .ObserveOn(RxApp.MainThreadScheduler)
                                .Subscribe(this.ResultsDataColumns.Add);
                        });

                    Type itemType = null;
                    Type itemWrapperType = null;
                    results
                        .ToObservable()
                        .Select(item =>
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
                            return itemWrapper;
                        })
                        .ObserveOn(RxApp.MainThreadScheduler)
                        .Subscribe(this.ResultsData.Add);
                }
            }
            catch (Exception ex)
            {
                ResultsError = ex.Message;
                return Unit.Default;
            }

            return Unit.Default;
        }
    }
}
