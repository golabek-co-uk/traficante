using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Traficante.Connect.Connectors;
using Traficante.Studio.ViewModels;
using Traficante.Studio.Views;

namespace Traficante.Studio.Models
{
    public class ElasticSearchObjectModel : ObjectModel, IConnectionObjectModel, IQueryableObjectModel
    {
        [DataMember]
        public ElasticSearchConnectionModel ConnectionInfo { get; set; }
        public override string Title => this.ConnectionInfo.Alias;
        public override object Icon => BaseLightIcons.Database;
        public string ConnectionAlias => this.ConnectionInfo.Alias;
        public QueryLanguageModel[] QueryLanguages => new[] { QueryLanguageModel.TraficantSQL };
        public ObservableCollection<object> QueryableItems => null;

        public ElasticSearchObjectModel()
        {
            ConnectionInfo = new ElasticSearchConnectionModel();
        }

        public ElasticSearchObjectModel(ElasticSearchConnectionModel connectionString)
        {
            ConnectionInfo = connectionString;
        }

        public override void LoadItems()
        {
            Observable
                .FromAsync(() => Task.Run(async () =>
                {
                    await new ElasticSearchConnector(this.ConnectionInfo.ToConectorConfig()).TryConnect();
                    return new object[] {
                        new ElasticSearchIndicesObjectModel(this),
                        new ElasticSearchAliasesObjectModel(this)
                    };
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

    public class ElasticSearchIndicesObjectModel : ObjectModel
    {
        public ElasticSearchObjectModel Server { get; }
        public override object Icon => BaseLightIcons.Folder;

        public ElasticSearchIndicesObjectModel(ElasticSearchObjectModel server)
        {
            Server = server;
            Title = "Indices";
        }

        public override void LoadItems()
        {
            Observable
                .FromAsync(() => Task.Run(async () => await new ElasticSearchConnector(Server.ConnectionInfo.ToConectorConfig()).GetIndices()))
                .SelectMany(x => x)
                .Select(x => new ElasticSearchIndexObjectModel(Server, x))
                .ObserveOn(RxApp.MainThreadScheduler)
                .Catch<object, Exception>(ex =>
                {
                    Interactions.Exceptions.Handle(ex).Subscribe();
                    return Observable.Empty<object>();
                })
                .Subscribe(x => Items.Add(x));
        }
    }

    public class ElasticSearchIndexObjectModel : ObjectModel, ITableObjectModel
    {
        public ElasticSearchObjectModel Server { get; }
        private volatile JsonDocument _index = null;
        public override object Icon => BaseLightIcons.Table;
        public string TableName => Title;
        public string[] TablePath 
        {
            get
            {
                if (_index != null)
                {
                    var mappingTypes = new ElasticSearchConnector(this.Server.ConnectionInfo.ToConectorConfig()).GetMappingTypes(this._index).ToList();
                    if (mappingTypes.Count > 1)
                        return new string[] { this.Server.Title, Title, mappingTypes.First() };
                }
                return new string[] { this.Server.Title, Title };
            }
        }

        public string[] TableFields
        {
            get
            {
                if (_index != null)
                {
                    var mappingTypes = new ElasticSearchConnector(this.Server.ConnectionInfo.ToConectorConfig()).GetMappingTypes(this._index).ToList();
                    var fields = new ElasticSearchConnector(this.Server.ConnectionInfo.ToConectorConfig()).GetFields(this._index).ToList();
                    if (mappingTypes.Count > 1)
                        return fields.Where(x => x.MappingType == mappingTypes[0]).Select(x => x.Name).ToArray();
                    else
                        return fields.Select(x => x.Name).ToArray();
                }
                return new string[0];
            }
        }

        public ElasticSearchIndexObjectModel(ElasticSearchObjectModel server, string name)
        {
            Server = server;
            Title = name;

            Task.Run(async () =>
            {
                this._index = await new ElasticSearchConnector(Server.ConnectionInfo.ToConectorConfig()).GetIndex(Title);
            })
            .ConfigureAwait(false);
        }

        public override void LoadItems()
        {
            Observable
                .FromAsync(() => Task.Run(async () =>
                {
                    _index = _index ?? await new ElasticSearchConnector(Server.ConnectionInfo.ToConectorConfig()).GetIndex(Title);
                    return _index
                        .RootElement
                        .GetProperty(this.Title)
                        .EnumerateObject()
                        .Select(x => new ElasticSearchJsonDocument(Server, x.Name, x.Value))
                        .ToList();
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

    public class ElasticSearchAliasesObjectModel : ObjectModel
    {
        public ElasticSearchObjectModel Server { get; }
        public override object Icon => BaseLightIcons.Folder;

        public ElasticSearchAliasesObjectModel(ElasticSearchObjectModel server)
        {
            Server = server;
            Title = "Aliases";
        }

        public override void LoadItems()
        {
            Observable
                .FromAsync(() => Task.Run(async () => await new ElasticSearchConnector(Server.ConnectionInfo.ToConectorConfig()).GetAliases()))
                .SelectMany(x => x)
                .Select(x => new ElasticSearchAliasObjectModel(Server, x))
                .ObserveOn(RxApp.MainThreadScheduler)
                .Catch<object, Exception>(ex =>
                {
                    Interactions.Exceptions.Handle(ex).Subscribe();
                    return Observable.Empty<object>();
                })
                .Subscribe(x => Items.Add(x));
        }
    }

    public class ElasticSearchAliasObjectModel : ObjectModel, ITableObjectModel
    {
        public ElasticSearchObjectModel Server { get; }
        public override object Icon => BaseLightIcons.Table;
        public string TableName => Title;
        public string[] TablePath => new string[] { this.Server.Title, Title };
        public string[] TableFields => new string[0];
        
        public ElasticSearchAliasObjectModel(ElasticSearchObjectModel server, string name)
        {
            Server = server;
            Title = name;
        }

        public override void LoadItems()
        {
            Observable
                .FromAsync(() => Task.Run(async () =>
                {
                    var index = await new ElasticSearchConnector(Server.ConnectionInfo.ToConectorConfig()).GetIndex(Title);
                    return index.RootElement
                    .EnumerateObject()
                    .Select(x => new ElasticSearchJsonDocument(Server, x.Name, x.Value))
                    .ToList();
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

    public class ElasticSearchJsonDocument : ObjectModel
    {
        public ElasticSearchObjectModel Server { get; }
        public JsonElement JsonElement { get; set; }
        public override object Icon => BaseLightIcons.Field;

        public ElasticSearchJsonDocument(ElasticSearchObjectModel server, string name, JsonElement jsonElement)
        {
            Server = server;
            JsonElement = jsonElement;
            Title = name;
            if (JsonElement.ValueKind != JsonValueKind.Object && JsonElement.ValueKind != JsonValueKind.Array)
            {
                Title = $"{name}: {jsonElement.ToString()}";
                Items = new ObservableCollection<object>();
            }
        }

        public override void LoadItems()
        {
            Observable
                .FromAsync(() => Task.Run(() =>
                {
                    if (JsonElement.ValueKind == JsonValueKind.Object)
                        return JsonElement
                            .EnumerateObject().ToList()
                            .Select(x => new ElasticSearchJsonDocument(Server, x.Name, x.Value))
                            .ToList();
                    if (JsonElement.ValueKind == JsonValueKind.Array)
                        return JsonElement
                            .EnumerateArray().ToList()
                            .Select((x, i) => new ElasticSearchJsonDocument(Server, $"[{i}]", x))
                            .ToList();
                    return new List<ElasticSearchJsonDocument>();
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
