using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Traficante.Connect;
using Traficante.Connect.Connectors;
using Traficante.Studio.Services;
using Traficante.Studio.ViewModels;
using Traficante.Studio.Views;

namespace Traficante.Studio.Models
{
    public class SqliteObjectModel : ObjectModel, IDataSourceObjectModel
    {
        [DataMember]
        public SqliteConnectionModel ConnectionInfo { get; set; }
        public override string Title => this.ConnectionInfo.Alias;
        public override object Icon => BaseLightIcons.Database;
        public string ConnectionAlias => this.ConnectionInfo.Alias;
        public ConnectorConfig ConnectorConfig => this.ConnectionInfo.ToConectorConfig();
        public QueryLanguage[] QueryLanguages => new[] { QueryLanguage.SqliteSQL };
        public ObservableCollection<IObjectModel> QueryableChildren => null;
        public bool HasQueryableChildren => false;
        public SqliteObjectModel() : base(null)
        {
            ConnectionInfo = new SqliteConnectionModel();
        }

        public SqliteObjectModel(SqliteConnectionModel connectionString) : base(null)
        {
            ConnectionInfo = connectionString;
        }

        public override void LoadChildren()
        {
            Observable
                .FromAsync(() => Task.Run(async () =>
                {
                    await new SqliteConnector(ConnectionInfo.ToConectorConfig()).TryConnect();
                    return new IObjectModel[] {
                        new SqliteTablesObjectModel(this),
                        new SqliteViewsObjectModel(this)
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

    public class SqliteTablesObjectModel : ObjectModel
    {
        public SqliteObjectModel Database { get; }
        public override string Title => "Tables";
        public override object Icon => BaseLightIcons.Folder;

        public SqliteTablesObjectModel(SqliteObjectModel database) : base(database)
        {
            Database = database;
        }

        public override void LoadChildren()
        {
            Observable
                .FromAsync(() => Task.Run(async () => await new SqliteConnector(Database.ConnectionInfo.ToConectorConfig()).GetTables()))
                .SelectMany(x => x)
                .Select(x => new SqliteTableObjectModel(Database, x))
                .ObserveOn(RxApp.MainThreadScheduler)
                .Catch<IObjectModel, Exception>(ex =>
                {
                    Interactions.Exceptions.Handle(ex).Subscribe();
                    return Observable.Empty<IObjectModel>();
                })
                .Subscribe(x => Children.Add(x));
        }
    }

    public class SqliteTableObjectModel : ObjectModel, ITableObjectModel
    {
        public SqliteObjectModel Database { get; }
        public override object Icon => BaseLightIcons.Table;
        public string TableName => Title;
        public string[] TablePath => new string[] { Database.Title, Title };
        public string[] TableFields => new string[0];
        
            public SqliteTableObjectModel(SqliteObjectModel databse, string name) : base(databse)
        {
            Database = databse;
            Title = name;
        }

        public override void LoadChildren()
        {
            Observable
                .FromAsync(() => Task.Run(async () => await new SqliteConnector(Database.ConnectionInfo.ToConectorConfig()).GetFields(this.Title)))
                .SelectMany(x => x)
                .Select(x => new SqliteFieldObjectModel(Database, x.Name, x.Type, x.NotNull))
                .ObserveOn(RxApp.MainThreadScheduler)
                .Catch<IObjectModel, Exception>(ex =>
                {
                    Interactions.Exceptions.Handle(ex).Subscribe();
                    return Observable.Empty<IObjectModel>();
                })
                .Subscribe(x => Children.Add(x));
        }
    }

    public class SqliteViewsObjectModel : ObjectModel
    {
        public SqliteObjectModel Database { get; }
        public override string Title => "Views";
        public override object Icon => BaseLightIcons.Folder;

        public SqliteViewsObjectModel(SqliteObjectModel database) : base(database)
        {
            Database = database;
        }

        public override void LoadChildren()
        {
            Observable
                .FromAsync(() => Task.Run(async () => await new SqliteConnector(Database.ConnectionInfo.ToConectorConfig()).GetViews()))
                .SelectMany(x => x)
                .Select(x => new SqliteViewObjectModel(Database, x))
                .ObserveOn(RxApp.MainThreadScheduler)
                .Catch<IObjectModel, Exception>(ex =>
                {
                    Interactions.Exceptions.Handle(ex).Subscribe();
                    return Observable.Empty<IObjectModel>();
                })
                .Subscribe(x => Children.Add(x));
        }
    }

    public class SqliteViewObjectModel : ObjectModel, ITableObjectModel
    {
        public SqliteObjectModel Database { get; }
        public string OnlyName { get; set; }
        public string OnlySchema { get; set; }
        public override object Icon => BaseLightIcons.Table;
        public string TableName => Title;
        public string[] TablePath => new string[] { Database.Title, OnlySchema, OnlyName };
        public string[] TableFields => new string[0];

        public SqliteViewObjectModel(SqliteObjectModel databse, string name) : base(databse)
        {
            Database = databse;
            Title = name;
        }

        public override void LoadChildren()
        {
            Observable
                .FromAsync(() => Task.Run(async () => await new SqliteConnector(Database.ConnectionInfo.ToConectorConfig()).GetFields(this.Title)))
                .SelectMany(x => x)
                .Select(x => new SqliteFieldObjectModel(Database, x.Name, x.Type, x.NotNull))
                .ObserveOn(RxApp.MainThreadScheduler)
                .Catch<IObjectModel, Exception>(ex =>
                {
                    Interactions.Exceptions.Handle(ex).Subscribe();
                    return Observable.Empty<IObjectModel>();
                })
                .Subscribe(x => Children.Add(x));
        }
    }

    public class SqliteFieldObjectModel : ObjectModel, IFieldObjectModel
    {
        public SqliteObjectModel Databse { get; }
        public string Name { get; set; }
        public override object Icon => BaseLightIcons.Field;
        public string FieldName => Name;
        public override ObservableCollection<IObjectModel> Children => null;

        public SqliteFieldObjectModel(SqliteObjectModel databse, string name, string type, bool? notNull) : base(databse)
        {
            Databse = databse;
            Title = $"{name} {type}";
            Name = name;
        }
    }

}
