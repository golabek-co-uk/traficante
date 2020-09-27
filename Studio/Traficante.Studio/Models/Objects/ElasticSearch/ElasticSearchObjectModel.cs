using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Runtime.Serialization;
using System.Text.Json;
using System.Threading.Tasks;
using Traficante.Connect;
using Traficante.Connect.Connectors;
using Traficante.Studio.Views;

namespace Traficante.Studio.Models
{
    public class ElasticSearchObjectModel : ObjectModel, IDataSourceObjectModel
    {
        [DataMember]
        public ElasticSearchConnectionModel ConnectionInfo { get; set; }
        public override string Title => this.ConnectionInfo.Alias;
        public override object Icon => BaseLightIcons.Database;
        public string ConnectionAlias => this.ConnectionInfo.Alias;
        public ConnectorConfig ConnectorConfig => this.ConnectionInfo.ToConectorConfig();
        public QueryLanguage[] QueryLanguages => new[] { QueryLanguage.TraficantSQL, QueryLanguage.ElasticSearchDSL };
        public ObservableCollection<IObjectModel> QueryableChildren => null;
        public bool HasQueryableChildren => false;

        public ElasticSearchObjectModel() : base(null)
        {
            ConnectionInfo = new ElasticSearchConnectionModel();
        }

        public ElasticSearchObjectModel(ElasticSearchConnectionModel connectionString) : base(null)
        {
            ConnectionInfo = connectionString;
        }

        public override void LoadChildren()
        {
            Observable
                .FromAsync(() => Task.Run(async () =>
                {
                    await new ElasticSearchConnector(this.ConnectionInfo.ToConectorConfig()).TryConnect();
                    return new IObjectModel[] {
                        new ElasticSearchIndicesObjectModel(this),
                        new ElasticSearchAliasesObjectModel(this)
                    };
                }))
                .SelectMany(x => x)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Catch<IObjectModel, Exception>(ex =>
                {
                    Interactions.Exceptions.Handle(ex).Subscribe();
                    return Observable.Empty<IObjectModel>();
                })
                .Subscribe(x => Children.Add(x));
        }
    }

    public class ElasticSearchIndicesObjectModel : ObjectModel
    {
        public ElasticSearchObjectModel Server { get; }
        public override object Icon => BaseLightIcons.Folder;

        public ElasticSearchIndicesObjectModel(ElasticSearchObjectModel server) : base (server)
        {
            Server = server;
            Title = "Indices";
        }

        public override void LoadChildren()
        {
            Observable
                .FromAsync(() => Task.Run(async () => await new ElasticSearchConnector(Server.ConnectionInfo.ToConectorConfig()).GetIndices()))
                .SelectMany(x => x)
                .Select(x => new ElasticSearchIndexObjectModel(Server, x))
                .ObserveOn(RxApp.MainThreadScheduler)
                .Catch<IObjectModel, Exception>(ex =>
                {
                    Interactions.Exceptions.Handle(ex).Subscribe();
                    return Observable.Empty<IObjectModel>();
                })
                .Subscribe(x => Children.Add(x));
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

        public ElasticSearchIndexObjectModel(ElasticSearchObjectModel server, string name) : base(server)
        {
            Server = server;
            Title = name;

            Task.Run(async () =>
            {
                this._index = await new ElasticSearchConnector(Server.ConnectionInfo.ToConectorConfig()).GetIndex(Title);
            })
            .ConfigureAwait(false);
        }

        public override void LoadChildren()
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
                .Catch<IObjectModel, Exception>(ex =>
                {
                    Interactions.Exceptions.Handle(ex).Subscribe();
                    return Observable.Empty<IObjectModel>();
                })
                .Subscribe(x => Children.Add(x));
        }
    }

    public class ElasticSearchAliasesObjectModel : ObjectModel
    {
        public ElasticSearchObjectModel Server { get; }
        public override object Icon => BaseLightIcons.Folder;

        public ElasticSearchAliasesObjectModel(ElasticSearchObjectModel server) : base(server)
        {
            Server = server;
            Title = "Aliases";
        }

        public override void LoadChildren()
        {
            Observable
                .FromAsync(() => Task.Run(async () => await new ElasticSearchConnector(Server.ConnectionInfo.ToConectorConfig()).GetAliases()))
                .SelectMany(x => x)
                .Select(x => new ElasticSearchAliasObjectModel(Server, x))
                .ObserveOn(RxApp.MainThreadScheduler)
                .Catch<IObjectModel, Exception>(ex =>
                {
                    Interactions.Exceptions.Handle(ex).Subscribe();
                    return Observable.Empty<IObjectModel>();
                })
                .Subscribe(x => Children.Add(x));
        }
    }

    public class ElasticSearchAliasObjectModel : ObjectModel, ITableObjectModel
    {
        public ElasticSearchObjectModel Server { get; }
        public override object Icon => BaseLightIcons.Table;
        public string TableName => Title;
        public string[] TablePath => new string[] { this.Server.Title, Title };
        public string[] TableFields => new string[0];
        
        public ElasticSearchAliasObjectModel(ElasticSearchObjectModel server, string name) : base(server)
        {
            Server = server;
            Title = name;
        }

        public override void LoadChildren()
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
                .Catch<IObjectModel, Exception>(ex =>
                {
                    Interactions.Exceptions.Handle(ex).Subscribe();
                    return Observable.Empty<IObjectModel>();
                })
                .Subscribe(x => Children.Add(x));
        }
    }

    public class ElasticSearchJsonDocument : ObjectModel
    {
        public ElasticSearchObjectModel Server { get; }
        public JsonElement JsonElement { get; set; }
        public override object Icon => BaseLightIcons.Field;

        public ElasticSearchJsonDocument(ElasticSearchObjectModel server, string name, JsonElement jsonElement) : base(server)
        {
            Server = server;
            JsonElement = jsonElement;
            Title = name;
            if (JsonElement.ValueKind != JsonValueKind.Object && JsonElement.ValueKind != JsonValueKind.Array)
            {
                Title = $"{name}: {jsonElement.ToString()}";
                Children = new ObservableCollection<IObjectModel>();
            }
        }

        public override void LoadChildren()
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
                .Catch<IObjectModel, Exception>(ex =>
                {
                    Interactions.Exceptions.Handle(ex).Subscribe();
                    return Observable.Empty<IObjectModel>();
                })
                .Subscribe(x => Children.Add(x));
        }
    }
}
