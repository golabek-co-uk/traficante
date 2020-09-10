using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Traficante.Connect.Connectors;
using Traficante.Studio.Services;
using Traficante.Studio.ViewModels;
using Traficante.Studio.Views;

namespace Traficante.Studio.Models
{
    public class FilesObjectModel : ObjectModel, IConnectionObjectModel, IQueryableObjectModel
    {
        [DataMember]
        [Reactive]
        public FilesConnectionModel ConnectionInfo { get; set; }
        public override object Icon => BaseLightIcons.Database;
        public override string Title => this.ConnectionInfo.Alias;
        public string ConnectionAlias => this.ConnectionInfo.Alias;
        public QueryLanguageModel[] QueryLanguages => new[] { QueryLanguageModel.TraficantSQL };
        public ObservableCollection<object> QueryableItems => null;

        public FilesObjectModel()
        {
            this.ConnectionInfo = new FilesConnectionModel();
        }

        public FilesObjectModel(FilesConnectionModel connectionInfo)
        {
            this.ConnectionInfo = connectionInfo;
        }

        public override void LoadItems()
        {
            Observable
                .FromAsync(() => Task.Run(() =>
                {
                    return ConnectionInfo
                        .Files
                        .Select(x =>
                        {
                            switch (x.Type)
                            {
                                case FileType.Csv:
                                   return (object)new CsvFileObjectModel(this, x);
                                case FileType.Excel:
                                    return (object)new ExcelFileObjectModel(this, x);
                                case FileType.Json:
                                    return (object)new JsonFileObjectModel(this, x);
                                case FileType.Xml:
                                    return (object)new XmlFileObjectModel(this, x);
                                case FileType.Text:
                                    return (object)new TextFileObjectModel(this, x);
                            }
                            return null;
                        })
                        .Where(x => x != null);
                }))
                .SelectMany(x => x)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Catch<object, Exception>(ex =>
                {
                    Interactions.Exceptions.Handle(ex).Subscribe();
                    return Observable.Empty<object>();
                })
                .Subscribe(x => Items.Add(x));
        }
    }
}
