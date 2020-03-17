using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reactive.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Traficante.Connect.Connectors;
using Traficante.Studio.Services;

namespace Traficante.Studio.Models
{
    public class MySqlObjectModel : ObjectModel
    {
        [DataMember]
        public MySqlConnectionModel ConnectionInfo { get; set; }
        public override string Name { get => this.ConnectionInfo.Alias; set { } }

        public MySqlObjectModel()
        {
            ConnectionInfo = new MySqlConnectionModel();
        }

        public MySqlObjectModel(MySqlConnectionModel connectionString)
        {
            ConnectionInfo = connectionString;
        }

        public override void LoadItems()
        {
            Observable
                .FromAsync(() => new TaskFactory().StartNew(() => new Traficante.Connect.Connectors.MySqlConnector(ConnectionInfo.ToConectorConfig()).GetDatabases()))
                .SelectMany(x => x)
                .Select(x => new MySqlDatabaseObjectModel(this, x))
                .ObserveOn(RxApp.MainThreadScheduler)
                .Catch<object, Exception>(ex =>
                {
                    Interactions.Exceptions.Handle(ex).Subscribe();
                    return Observable.Empty<object>();
                })
                .Subscribe(x => Items.Add(x));
        }
    }

    public class MySqlDatabaseObjectModel : ObjectModel
    {
        public MySqlObjectModel Server { get; }

        public MySqlDatabaseObjectModel(MySqlObjectModel server, string name)
        {
            Server = server;
            Name = name;
        }

        public override void LoadItems()
        {
            Observable
                .FromAsync(() => new TaskFactory().StartNew(() =>
                {
                    new Traficante.Connect.Connectors.MySqlConnector(Server.ConnectionInfo.ToConectorConfig()).TryConnect(Name);
                    return new object[] {
                        new MySqlTablesObjectModel(this),
                        new MySqlViewsObjectModel(this)
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

    public class MySqlTablesObjectModel : ObjectModel
    {
        public MySqlDatabaseObjectModel Database { get; }

        public MySqlTablesObjectModel(MySqlDatabaseObjectModel database)
        {
            Database = database;
            Name = "Tables";
        }

        public override void LoadItems()
        {
            Observable
                .FromAsync(() => new TaskFactory().StartNew(() => new Traficante.Connect.Connectors.MySqlConnector(Database.Server.ConnectionInfo.ToConectorConfig()).GetTables(Database.Name)))
                .SelectMany(x => x)
                .Select(x => new MySqlTableObjectModel(Database, x))
                .ObserveOn(RxApp.MainThreadScheduler)
                .Catch<object, Exception>(ex =>
                {
                    Interactions.Exceptions.Handle(ex).Subscribe();
                    return Observable.Empty<object>();
                })
                .Subscribe(x => Items.Add(x));
        }
    }

    public class MySqlTableObjectModel : ObjectModel, IObjectPath, IObjectFields
    {
        public MySqlDatabaseObjectModel Databse { get; }
        
        public MySqlTableObjectModel(MySqlDatabaseObjectModel databse, string name)
        {
            Databse = databse;
            Name = name;
        }

        public override void LoadItems()
        {
        }

        public string[] GetObjectPath()
        {
            return new string[] { Databse.Server.Name, Databse.Name, Name };
        }

        public string[] GetObjectFields()
        {
            return new string[0];
        }
    }

    public class MySqlViewsObjectModel : ObjectModel
    {
        public MySqlDatabaseObjectModel Database { get; }

        public MySqlViewsObjectModel(MySqlDatabaseObjectModel database)
        {
            Database = database;
            Name = "Views";
        }

        public override void LoadItems()
        {
            Observable
                .FromAsync(() => new TaskFactory().StartNew(() => new Traficante.Connect.Connectors.MySqlConnector(Database.Server.ConnectionInfo.ToConectorConfig()).GetViews(Database.Name)))
                .SelectMany(x => x)
                .Select(x => new MySqlViewObjectModel(Database, x))
                .ObserveOn(RxApp.MainThreadScheduler)
                .Catch<object, Exception>(ex =>
                {
                    Interactions.Exceptions.Handle(ex).Subscribe();
                    return Observable.Empty<object>();
                })
                .Subscribe(x => Items.Add(x));
        }
    }

    public class MySqlViewObjectModel : ObjectModel, IObjectPath, IObjectFields
    {
        public MySqlDatabaseObjectModel Databse { get; }

        public MySqlViewObjectModel(MySqlDatabaseObjectModel databse, string name)
        {
            Databse = databse;
            Name = name;
        }

        public override void LoadItems()
        {
        }

        public string[] GetObjectPath()
        {
            return new string[] { Databse.Server.Name, Databse.Name, Name };
        }

        public string[] GetObjectFields()
        {
            return new string[0];
        }
    }

    [DataContract]
    public class MySqlConnectionModel : ReactiveObject
    {
        [DataMember]
        public string Alias { get; set; }

        [DataMember]
        public string Server { get; set; }
        [DataMember]
        public string UserId { get; set; }
        [DataMember]
        public string Password { get; set; }

        public MySqlConnectorConfig ToConectorConfig()
        {
            return new MySqlConnectorConfig()
            {
                Alias = this.Alias,
                Server = this.Server,
                UserId = this.UserId,
                Password = this.Password
            };
        }
    }
}
