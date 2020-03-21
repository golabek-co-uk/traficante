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

namespace Traficante.Studio.Models
{
    public class ElasticSearchObjectModel : ObjectModel
    {
        [DataMember]
        public ElasticSearchConnectionModel ConnectionInfo { get; set; }
        public override string Name
        {
            get { return this.ConnectionInfo.Alias; }
            set { }
        }

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
                .FromAsync(() => new TaskFactory().StartNew(() =>
                {
                    new ElasticSearchConnector(this.ConnectionInfo.ToConectorConfig()).TryConnect();
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

        public ElasticSearchIndicesObjectModel(ElasticSearchObjectModel server)
        {
            Server = server;
            Name = "Indices";
        }

        public override void LoadItems()
        {
            Observable
                .FromAsync(() => new TaskFactory().StartNew(() => new ElasticSearchConnector(Server.ConnectionInfo.ToConectorConfig()).GetIndices()))
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

    public class ElasticSearchIndexObjectModel : ObjectModel, IObjectSource
    {
        public ElasticSearchObjectModel Server { get; }

        public ElasticSearchIndexObjectModel(ElasticSearchObjectModel server, string name)
        {
            Server = server;
            Name = name;
        }

        public override void LoadItems()
        {
            Observable
                .FromAsync(() => new TaskFactory().StartNew(() =>
                {
                    return new ElasticSearchConnector(Server.ConnectionInfo.ToConectorConfig())
                    .GetIndexMapping(Name)
                    .RootElement.GetProperty(this.Name)
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

        public string[] GetObjectPath()
        {
            return new string[] { Name };
        }

        public string[] GetObjectFields()
        {
            return new string[0];
        }
    }

    public class ElasticSearchAliasesObjectModel : ObjectModel
    {
        public ElasticSearchObjectModel Server { get; }

        public ElasticSearchAliasesObjectModel(ElasticSearchObjectModel server)
        {
            Server = server;
            Name = "Aliases";
        }

        public override void LoadItems()
        {
            Observable
                .FromAsync(() => new TaskFactory().StartNew(() => new ElasticSearchConnector(Server.ConnectionInfo.ToConectorConfig()).GetAliases()))
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

    public class ElasticSearchAliasObjectModel : ObjectModel, IObjectSource
    {
        public ElasticSearchObjectModel Server { get; }

        public ElasticSearchAliasObjectModel(ElasticSearchObjectModel server, string name)
        {
            Server = server;
            Name = name;
        }

        public override void LoadItems()
        {
            Observable
                .FromAsync(() => new TaskFactory().StartNew(() =>
                {
                    return new ElasticSearchConnector(Server.ConnectionInfo.ToConectorConfig())
                    .GetIndexMapping(Name)
                    .RootElement
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

        public string[] GetObjectPath()
        {
            return new string[] { Name };
        }

        public string[] GetObjectFields()
        {
            return new string[0];
        }
    }

    public class ElasticSearchJsonDocument : ObjectModel
    {
        public ElasticSearchObjectModel Server { get; }
        public JsonElement JsonElement { get; set; }

        public ElasticSearchJsonDocument(ElasticSearchObjectModel server, string name, JsonElement jsonElement)
        {
            Server = server;
            JsonElement = jsonElement;
            Name = name;
            if (JsonElement.ValueKind != JsonValueKind.Object && JsonElement.ValueKind != JsonValueKind.Array)
            {
                Name = $"{name}: {jsonElement.ToString()}";
                Items = new ObservableCollection<object>();
            }
        }

        public override void LoadItems()
        {
            Observable
                .FromAsync(() => new TaskFactory().StartNew(() =>
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

    [DataContract]
    public class ElasticSearchConnectionModel : ReactiveObject
    {
        [DataMember]
        public string Alias { get; set; }

        [DataMember]
        public string Server { get; set; }
        
        public ElasticSearchConnectorConfig ToConectorConfig()
        {
            return new ElasticSearchConnectorConfig()
            {
                Alias = this.Alias,
                Server = this.Server
            };
        }
    }
}
