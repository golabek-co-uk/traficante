using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Traficante.Connect.Connectors;
using Traficante.Studio.Services;

namespace Traficante.Studio.Models
{
    public class SqlServerObjectModel : ObjectModel, IAliasObjectModel
    {
        [DataMember]
        public SqlServerConnectionModel ConnectionInfo { get; set; }
        public override string Title => this.ConnectionInfo.Alias;

        public SqlServerObjectModel()
        {
            ConnectionInfo = new SqlServerConnectionModel();
        }
        
        public SqlServerObjectModel(SqlServerConnectionModel connectionString)
        {
            ConnectionInfo = connectionString;
        }
        
        public override void LoadItems()
        {
            Observable
                .FromAsync(() => Task.Run(async () => await new SqlServerConnector(ConnectionInfo.ToConectorConfig()).GetDatabases()))
                .SelectMany(x => x)
                .Select(x => new SqlServerDatabaseObjectModel(this, x))
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

    public class SqlServerDatabaseObjectModel : ObjectModel
    {
        public SqlServerObjectModel Server { get; }

        public SqlServerDatabaseObjectModel(SqlServerObjectModel sqlServer, string name)
        {
            Server = sqlServer;
            Title = name;
        }

        public override void LoadItems()
        {
            Observable
                .FromAsync(() => Task.Run(async () =>
                {
                    await new SqlServerConnector(Server.ConnectionInfo.ToConectorConfig()).TryConnect(Title);
                    return new object[] {
                        new SqlServerTablesObjectModel(this),
                        new SqlServerViewsObjectModel(this)
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

    public class SqlServerTablesObjectModel : ObjectModel
    {
        public SqlServerDatabaseObjectModel Database { get; }

        public SqlServerTablesObjectModel(SqlServerDatabaseObjectModel database)
        {
            Database = database;
            Title = "Tables";
        }

        public override void LoadItems()
        {
            Observable
                .FromAsync(() => Task.Run(async () => await new SqlServerConnector(Database.Server.ConnectionInfo.ToConectorConfig()).GetTables(Database.Title)))
                .SelectMany(x => x)
                .Select(x => new SqlServerTableObjectModel(Database, x.Schema, x.Name))
                .ObserveOn(RxApp.MainThreadScheduler)
                .Catch<object, Exception>(ex =>
                {
                    Interactions.Exceptions.Handle(ex).Subscribe();
                    return Observable.Empty<object>();
                })
                .Subscribe(x => Items.Add(x));
        }
    }

    public class SqlServerTableObjectModel : ObjectModel, ITableObjectModel
    {
        public SqlServerDatabaseObjectModel Database { get; }
        public string OnlyName { get; set; }
        public string OnlySchema { get; set; }

        public SqlServerTableObjectModel(SqlServerDatabaseObjectModel databse, string schema, string name)
        {
            Database = databse;
            OnlyName = name;
            OnlySchema = schema;
            Title = schema + "." + name;
        }

        public override void LoadItems()
        {
            Observable
                .FromAsync(() => Task.Run(async () => await new SqlServerConnector(Database.Server.ConnectionInfo.ToConectorConfig()).GetFields(this.Database.Title, this.OnlyName)))
                .SelectMany(x => x)
                .Select(x => new SqlServerFieldObjectModel(Database, x.Name, x.Type, x.NotNull))
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
            return new string[] { Database.Server.Title, Database.Title, OnlySchema, OnlyName };
        }

        public string[] GetTableFields()
        {
            return new string[0];
        }
    }

    public class SqlServerViewsObjectModel : ObjectModel
    {
        public SqlServerDatabaseObjectModel Database { get; }

        public SqlServerViewsObjectModel(SqlServerDatabaseObjectModel database)
        {
            Database = database;
            Title = "Views";
        }

        public override void LoadItems()
        {
            Observable
                .FromAsync(() => Task.Run(async () => await new SqlServerConnector(Database.Server.ConnectionInfo.ToConectorConfig()).GetViews(Database.Title)))
                .SelectMany(x => x)
                .Select(x => new SqlServerViewObjectModel(Database, x.Schema, x.Name))
                .ObserveOn(RxApp.MainThreadScheduler)
                .Catch<object, Exception>(ex =>
                {
                    Interactions.Exceptions.Handle(ex).Subscribe();
                    return Observable.Empty<object>();
                })
                .Subscribe(x => Items.Add(x));
        }
    }

    public class SqlServerViewObjectModel : ObjectModel, ITableObjectModel
    {
        public SqlServerDatabaseObjectModel Database { get; }
        public string OnlyName { get; set; }
        public string OnlySchema { get; set; }

        public SqlServerViewObjectModel(SqlServerDatabaseObjectModel databse, string schema, string name)
        {
            Database = databse;
            OnlyName = name;
            OnlySchema = schema;
            Title = schema + "." + name;
        }

        public override void LoadItems()
        {
            Observable
                .FromAsync(() => Task.Run(async () => await new SqlServerConnector(Database.Server.ConnectionInfo.ToConectorConfig()).GetFields(this.Database.Title, this.OnlyName)))
                .SelectMany(x => x)
                .Select(x => new SqlServerFieldObjectModel(Database, x.Name, x.Type, x.NotNull))
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
            return new string[] { Database.Server.Title, Database.Title, OnlySchema, OnlyName };
        }

        public string[] GetTableFields()
        {
            return new string[0];
        }
    }

    public class SqlServerFieldObjectModel : ObjectModel, IFieldObjectModel
    {
        public SqlServerDatabaseObjectModel Databse { get; }
        public string NameOnly { get; set; }

        public SqlServerFieldObjectModel(SqlServerDatabaseObjectModel databse, string name, string type, bool? notNull)
        {
            Databse = databse;
            Title = $"{name} {type}";
            NameOnly = name;
        }

        public override ObservableCollection<object> Items => null;

        public string GetFieldName()
        {
            return this.NameOnly;
        }
    }

}
