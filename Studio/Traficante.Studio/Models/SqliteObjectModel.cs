using ReactiveUI;
using System;
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
    public class SqliteObjectModel : ObjectModel
    {
        [DataMember]
        public SqliteConnectionModel ConnectionInfo { get; set; }
        public override string Title
        {
            get { return this.ConnectionInfo.Alias; }
            set { }
        }

        public SqliteObjectModel()
        {
            ConnectionInfo = new SqliteConnectionModel();
        }

        public SqliteObjectModel(SqliteConnectionModel connectionString)
        {
            ConnectionInfo = connectionString;
        }

        public override void LoadItems()
        {
            Observable
                .FromAsync(() => Task.Run(async () =>
                {
                    await new SqliteConnector(ConnectionInfo.ToConectorConfig()).TryConnect();
                    return new object[] {
                        new SqliteTablesObjectModel(this),
                        new SqliteViewsObjectModel(this)
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

    public class SqliteTablesObjectModel : ObjectModel
    {
        public SqliteObjectModel Database { get; }

        public SqliteTablesObjectModel(SqliteObjectModel database)
        {
            Database = database;
            Title = "Tables";
        }

        public override void LoadItems()
        {
            Observable
                .FromAsync(() => Task.Run(async () => await new SqliteConnector(Database.ConnectionInfo.ToConectorConfig()).GetTables()))
                .SelectMany(x => x)
                .Select(x => new SqliteTableObjectModel(Database, x))
                .ObserveOn(RxApp.MainThreadScheduler)
                .Catch<object, Exception>(ex =>
                {
                    Interactions.Exceptions.Handle(ex).Subscribe();
                    return Observable.Empty<object>();
                })
                .Subscribe(x => Items.Add(x));
        }
    }

    public class SqliteTableObjectModel : ObjectModel, ITableObjectModel
    {
        public SqliteObjectModel Database { get; }

        public SqliteTableObjectModel(SqliteObjectModel databse, string name)
        {
            Database = databse;
            Title = name;
        }

        public override void LoadItems()
        {
            Observable
                .FromAsync(() => Task.Run(async () => await new SqliteConnector(Database.ConnectionInfo.ToConectorConfig()).GetFields(this.Title)))
                .SelectMany(x => x)
                .Select(x => new SqliteFieldObjectModel(Database, x.Name, x.Type, x.NotNull))
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
            return new string[] { Database.Title, Title };
        }

        public string[] GetTableFields()
        {
            return new string[0];
        }
    }

    public class SqliteViewsObjectModel : ObjectModel
    {
        public SqliteObjectModel Database { get; }

        public SqliteViewsObjectModel(SqliteObjectModel database)
        {
            Database = database;
            Title = "Views";
        }

        public override void LoadItems()
        {
            Observable
                .FromAsync(() => Task.Run(async () => await new SqliteConnector(Database.ConnectionInfo.ToConectorConfig()).GetViews()))
                .SelectMany(x => x)
                .Select(x => new SqliteViewObjectModel(Database, x))
                .ObserveOn(RxApp.MainThreadScheduler)
                .Catch<object, Exception>(ex =>
                {
                    Interactions.Exceptions.Handle(ex).Subscribe();
                    return Observable.Empty<object>();
                })
                .Subscribe(x => Items.Add(x));
        }
    }

    public class SqliteViewObjectModel : ObjectModel, ITableObjectModel
    {
        public SqliteObjectModel Database { get; }
        public string OnlyName { get; set; }
        public string OnlySchema { get; set; }

        public SqliteViewObjectModel(SqliteObjectModel databse, string name)
        {
            Database = databse;
            Title = name;
        }

        public override void LoadItems()
        {
            Observable
                .FromAsync(() => Task.Run(async () => await new SqliteConnector(Database.ConnectionInfo.ToConectorConfig()).GetFields(this.Title)))
                .SelectMany(x => x)
                .Select(x => new SqliteFieldObjectModel(Database, x.Name, x.Type, x.NotNull))
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
            return new string[] { Database.Title, OnlySchema, OnlyName };
        }

        public string[] GetTableFields()
        {
            return new string[0];
        }
    }

    public class SqliteFieldObjectModel : ObjectModel, IFieldObjectModel
    {
        public SqliteObjectModel Databse { get; }
        public string Name { get; set; }

        public SqliteFieldObjectModel(SqliteObjectModel databse, string name, string type, bool? notNull)
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

    [DataContract]
    public class SqliteConnectionModel : ReactiveObject
    {
        [DataMember]
        public string Alias { get; set; }

        [DataMember]
        public string Database { get; set; }
        
        public SqliteConnectorConfig ToConectorConfig()
        {
            return new SqliteConnectorConfig()
            {
                Alias = this.Alias,
                Database = this.Database,
            };
        }
    }

}
