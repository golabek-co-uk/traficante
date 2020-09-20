using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reactive.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Traficante.Connect;
using Traficante.Connect.Connectors;
using Traficante.Studio.Services;
using Traficante.Studio.ViewModels;
using Traficante.Studio.Views;

namespace Traficante.Studio.Models
{
    public class MySqlObjectModel : ObjectModel, IConnectionObjectModel, IQueryableObjectModel
    {
        [DataMember]
        public MySqlConnectionModel ConnectionInfo { get; set; }
        public override string Title => this.ConnectionInfo.Alias;
        public override object Icon => BaseLightIcons.Database;
        public string ConnectionAlias => this.ConnectionInfo.Alias;
        public ConnectorConfig ConnectorConfig => this.ConnectionInfo.ToConectorConfig();
        public QueryLanguage[] QueryLanguages => new[] { QueryLanguage.MySQLSQL };
        public ObservableCollection<IObjectModel> QueryableChildren => Children;

        public MySqlObjectModel() : base(null)
        {
            ConnectionInfo = new MySqlConnectionModel();
        }

        public MySqlObjectModel(MySqlConnectionModel connectionString) : base(null)
        {
            ConnectionInfo = connectionString;
        }

        public override void LoadChildren()
        {
            Observable
                .FromAsync(() => Task.Run(async () => await new Traficante.Connect.Connectors.MySqlConnector(ConnectionInfo.ToConectorConfig()).GetDatabases()))
                .SelectMany(x => x)
                .Select(x => new MySqlDatabaseObjectModel(this, x))
                .ObserveOn(RxApp.MainThreadScheduler)
                .Catch<IObjectModel, Exception>(ex =>
                {
                    Interactions.Exceptions.Handle(ex).Subscribe();
                    return Observable.Empty<IObjectModel>();
                })
                .Subscribe(x => Children.Add(x));
        }
    }

    public class MySqlDatabaseObjectModel : ObjectModel, IQueryableObjectModel
    {
        public MySqlObjectModel Server { get; }
        public override object Icon => BaseLightIcons.Database;
        public QueryLanguage[] QueryLanguages => new[] { QueryLanguage.MySQLSQL };
        public ObservableCollection<IObjectModel> QueryableChildren => null;

        public MySqlDatabaseObjectModel(MySqlObjectModel server, string name) : base(server)
        {
            Server = server;
            Title = name;
        }

        public override void LoadChildren()
        {
            Observable
                .FromAsync(() => Task.Run(async () =>
                {
                    await new Traficante.Connect.Connectors.MySqlConnector(Server.ConnectionInfo.ToConectorConfig()).TryConnect(Title);
                    return new IObjectModel[] {
                        new MySqlTablesObjectModel(this),
                        new MySqlViewsObjectModel(this)
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

    public class MySqlTablesObjectModel : ObjectModel
    {
        public MySqlDatabaseObjectModel Database { get; }
        public override object Icon => BaseLightIcons.Folder;

        public MySqlTablesObjectModel(MySqlDatabaseObjectModel database) : base(database)
        {
            Database = database;
            Title = "Tables";
        }

        public override void LoadChildren()
        {
            Observable
                .FromAsync(() => Task.Run(async () => await new Traficante.Connect.Connectors.MySqlConnector(Database.Server.ConnectionInfo.ToConectorConfig()).GetTables(Database.Title)))
                .SelectMany(x => x)
                .Select(x => new MySqlTableObjectModel(Database, x))
                .ObserveOn(RxApp.MainThreadScheduler)
                .Catch<IObjectModel, Exception>(ex =>
                {
                    Interactions.Exceptions.Handle(ex).Subscribe();
                    return Observable.Empty<IObjectModel>();
                })
                .Subscribe(x => Children.Add(x));
        }
    }

    public class MySqlTableObjectModel : ObjectModel, ITableObjectModel
    {
        public MySqlDatabaseObjectModel Database { get; }
        public override object Icon => BaseLightIcons.Table;
        public string TableName => Title;
        public string[] TablePath => new string[] { Database.Server.Title, Database.Title, Title };
        public string[] TableFields => new string[0];

        public MySqlTableObjectModel(MySqlDatabaseObjectModel databse, string name) : base(databse)
        {
            Database = databse;
            Title = name;
        }

        public override void LoadChildren()
        {
            Observable
                .FromAsync(() => Task.Run(async () => await new MySqlConnector(Database.Server.ConnectionInfo.ToConectorConfig()).GetFields(this.Database.Title, this.Title)))
                .SelectMany(x => x)
                .Select(x => new MySqlFieldObjectModel(Database, x.Name, x.Type, x.NotNull))
                .ObserveOn(RxApp.MainThreadScheduler)
                .Catch<IObjectModel, Exception>(ex =>
                {
                    Interactions.Exceptions.Handle(ex).Subscribe();
                    return Observable.Empty<IObjectModel>();
                })
                .Subscribe(x => Children.Add(x));
        }
    }

    public class MySqlViewsObjectModel : ObjectModel
    {
        public MySqlDatabaseObjectModel Database { get; }
        public override object Icon => BaseLightIcons.Folder;

        public MySqlViewsObjectModel(MySqlDatabaseObjectModel database) : base(database)
        {
            Database = database;
            Title = "Views";
        }

        public override void LoadChildren()
        {
            Observable
                .FromAsync(() => Task.Run(async () => await new Traficante.Connect.Connectors.MySqlConnector(Database.Server.ConnectionInfo.ToConectorConfig()).GetViews(Database.Title)))
                .SelectMany(x => x)
                .Select(x => new MySqlViewObjectModel(Database, x))
                .ObserveOn(RxApp.MainThreadScheduler)
                .Catch<IObjectModel, Exception>(ex =>
                {
                    Interactions.Exceptions.Handle(ex).Subscribe();
                    return Observable.Empty<IObjectModel>();
                })
                .Subscribe(x => Children.Add(x));
        }
    }

    public class MySqlViewObjectModel : ObjectModel, ITableObjectModel
    {
        public MySqlDatabaseObjectModel Database { get; }
        public override object Icon => BaseLightIcons.Table;
        public string TableName => Title;
        public string[] TablePath => new string[] { Database.Server.Title, Database.Title, Title };
        public string[] TableFields => new string[0];

        public MySqlViewObjectModel(MySqlDatabaseObjectModel databse, string name) : base(databse)
        {
            Database = databse;
            Title = name;
        }

        public override void LoadChildren()
        {
            Observable
                .FromAsync(() => Task.Run(async () => await new MySqlConnector(Database.Server.ConnectionInfo.ToConectorConfig()).GetFields(Database.Title, this.Title)))
                .SelectMany(x => x)
                .Select(x => new MySqlFieldObjectModel(Database, x.Name, x.Type, x.NotNull))
                .ObserveOn(RxApp.MainThreadScheduler)
                .Catch<IObjectModel, Exception>(ex =>
                {
                    Interactions.Exceptions.Handle(ex).Subscribe();
                    return Observable.Empty<IObjectModel>();
                })
                .Subscribe(x => Children.Add(x));
        }
    }

    public class MySqlFieldObjectModel : ObjectModel, IFieldObjectModel
    {
        public MySqlDatabaseObjectModel Databse { get; }
        public string Name { get; set; }
        public override object Icon => BaseLightIcons.Field;
        public string FieldName => Name;
        public override ObservableCollection<IObjectModel> Children => null;

        public MySqlFieldObjectModel(MySqlDatabaseObjectModel databse, string name, string type, bool? notNull) : base(databse)
        {
            Databse = databse;
            Title = $"{name} {type}";
            Name = name;
        }
    }
}
