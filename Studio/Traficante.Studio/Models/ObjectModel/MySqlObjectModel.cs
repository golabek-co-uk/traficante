﻿using Newtonsoft.Json;
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
using Traficante.Studio.Views;

namespace Traficante.Studio.Models
{
    public class MySqlObjectModel : ObjectModel, IAliasObjectModel
    {
        [DataMember]
        public MySqlConnectionModel ConnectionInfo { get; set; }
        public override string Title => this.ConnectionInfo.Alias;
        public override object Icon => Icons.Database;

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
                .FromAsync(() => Task.Run(async () => await new Traficante.Connect.Connectors.MySqlConnector(ConnectionInfo.ToConectorConfig()).GetDatabases()))
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

        public string GetAlias()
        {
            return this.ConnectionInfo.Alias;
        }
    }

    public class MySqlDatabaseObjectModel : ObjectModel
    {
        public MySqlObjectModel Server { get; }
        public override object Icon => Icons.Database;

        public MySqlDatabaseObjectModel(MySqlObjectModel server, string name)
        {
            Server = server;
            Title = name;
        }

        public override void LoadItems()
        {
            Observable
                .FromAsync(() => Task.Run(async () =>
                {
                    await new Traficante.Connect.Connectors.MySqlConnector(Server.ConnectionInfo.ToConectorConfig()).TryConnect(Title);
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
        public override object Icon => Icons.Folder;

        public MySqlTablesObjectModel(MySqlDatabaseObjectModel database)
        {
            Database = database;
            Title = "Tables";
        }

        public override void LoadItems()
        {
            Observable
                .FromAsync(() => Task.Run(async () => await new Traficante.Connect.Connectors.MySqlConnector(Database.Server.ConnectionInfo.ToConectorConfig()).GetTables(Database.Title)))
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

    public class MySqlTableObjectModel : ObjectModel, ITableObjectModel
    {
        public MySqlDatabaseObjectModel Database { get; }
        public override object Icon => Icons.Table;

        public MySqlTableObjectModel(MySqlDatabaseObjectModel databse, string name)
        {
            Database = databse;
            Title = name;
        }

        public override void LoadItems()
        {
            Observable
                .FromAsync(() => Task.Run(async () => await new MySqlConnector(Database.Server.ConnectionInfo.ToConectorConfig()).GetFields(this.Database.Title, this.Title)))
                .SelectMany(x => x)
                .Select(x => new MySqlFieldObjectModel(Database, x.Name, x.Type, x.NotNull))
                .ObserveOn(RxApp.MainThreadScheduler)
                .Catch<object, Exception>(ex =>
                {
                    Interactions.Exceptions.Handle(ex).Subscribe();
                    return Observable.Empty<object>();
                })
                .Subscribe(x => Items.Add(x));
        }

        public string[] GetTablePath()
        {
            return new string[] { Database.Server.Title, Database.Title, Title };
        }

        public string[] GetTableFields()
        {
            return new string[0];
        }
    }

    public class MySqlViewsObjectModel : ObjectModel
    {
        public MySqlDatabaseObjectModel Database { get; }
        public override object Icon => Icons.Folder;

        public MySqlViewsObjectModel(MySqlDatabaseObjectModel database)
        {
            Database = database;
            Title = "Views";
        }

        public override void LoadItems()
        {
            Observable
                .FromAsync(() => Task.Run(async () => await new Traficante.Connect.Connectors.MySqlConnector(Database.Server.ConnectionInfo.ToConectorConfig()).GetViews(Database.Title)))
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

    public class MySqlViewObjectModel : ObjectModel, ITableObjectModel
    {
        public MySqlDatabaseObjectModel Database { get; }
        public override object Icon => Icons.Table;

        public MySqlViewObjectModel(MySqlDatabaseObjectModel databse, string name)
        {
            Database = databse;
            Title = name;
        }

        public override void LoadItems()
        {
            Observable
                .FromAsync(() => Task.Run(async () => await new MySqlConnector(Database.Server.ConnectionInfo.ToConectorConfig()).GetFields(Database.Title, this.Title)))
                .SelectMany(x => x)
                .Select(x => new MySqlFieldObjectModel(Database, x.Name, x.Type, x.NotNull))
                .ObserveOn(RxApp.MainThreadScheduler)
                .Catch<object, Exception>(ex =>
                {
                    Interactions.Exceptions.Handle(ex).Subscribe();
                    return Observable.Empty<object>();
                })
                .Subscribe(x => Items.Add(x));
        }

        public string[] GetTablePath()
        {
            return new string[] { Database.Server.Title, Database.Title, Title };
        }

        public string[] GetTableFields()
        {
            return new string[0];
        }
    }

    public class MySqlFieldObjectModel : ObjectModel, IFieldObjectModel
    {
        public MySqlDatabaseObjectModel Databse { get; }
        public string Name { get; set; }
        public override object Icon => Icons.Field;

        public MySqlFieldObjectModel(MySqlDatabaseObjectModel databse, string name, string type, bool? notNull)
        {
            Databse = databse;
            Title = $"{name} {type}";
            Name = name;
        }

        public override ObservableCollection<object> Items => null;

        public string GetFieldName()
        {
            return Name;
        }
    }
}
