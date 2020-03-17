using ReactiveUI;
using System;
using System.Collections.Generic;
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
    public class SqlServerObjectModel : ObjectModel
    {
        [DataMember]
        public SqlServerConnectionModel ConnectionInfo { get; set; }
        public override string Name {
            get { return this.ConnectionInfo.Alias; }
            set { } 
        }

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
                .FromAsync(() => new TaskFactory().StartNew(() => new SqlServerConnector(ConnectionInfo.ToConectorConfig()).GetDatabases()))
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
    }

    public class SqlServerDatabaseObjectModel : ObjectModel
    {
        public SqlServerObjectModel Server { get; }

        public SqlServerDatabaseObjectModel(SqlServerObjectModel sqlServer, string name)
        {
            Server = sqlServer;
            Name = name;
        }

        public override void LoadItems()
        {
            Observable
                .FromAsync(() => new TaskFactory().StartNew(() =>
                {
                    new SqlServerConnector(Server.ConnectionInfo.ToConectorConfig()).TryConnect(Name);
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
            Name = "Tables";
        }

        public override void LoadItems()
        {
            Observable
                .FromAsync(() => new TaskFactory().StartNew(() => new SqlServerConnector(Database.Server.ConnectionInfo.ToConectorConfig()).GetTables(Database.Name)))
                .SelectMany(x => x)
                .Select(x => new SqlServerTableObjectModel(Database, x.schema, x.name))
                .ObserveOn(RxApp.MainThreadScheduler)
                .Catch<object, Exception>(ex =>
                {
                    Interactions.Exceptions.Handle(ex).Subscribe();
                    return Observable.Empty<object>();
                })
                .Subscribe(x => Items.Add(x));
        }
    }

    public class SqlServerTableObjectModel : ObjectModel, IObjectPath, IObjectFields
    {
        public SqlServerDatabaseObjectModel Databse { get; }
        public string OnlyName { get; set; }
        public string OnlySchema { get; set; }

        public SqlServerTableObjectModel(SqlServerDatabaseObjectModel databse, string schema, string name)
        {
            Databse = databse;
            OnlyName = name;
            OnlySchema = schema;
            Name = schema + "." + name;
        }

        public override void LoadItems()
        {
        }

        public string[] GetObjectPath()
        {
            return new string[] { Databse.Server.Name, Databse.Name, OnlySchema, OnlyName };
        }

        public string[] GetObjectFields()
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
            Name = "Views";
        }

        public override void LoadItems()
        {
            Observable
                .FromAsync(() => new TaskFactory().StartNew(() => new SqlServerConnector(Database.Server.ConnectionInfo.ToConectorConfig()).GetViews(Database.Name)))
                .SelectMany(x => x)
                .Select(x => new SqlServerViewObjectModel(Database, x.schema, x.name))
                .ObserveOn(RxApp.MainThreadScheduler)
                .Catch<object, Exception>(ex =>
                {
                    Interactions.Exceptions.Handle(ex).Subscribe();
                    return Observable.Empty<object>();
                })
                .Subscribe(x => Items.Add(x));
        }
    }
    public class SqlServerViewObjectModel : ObjectModel, IObjectPath, IObjectFields
    {
        public SqlServerDatabaseObjectModel Databse { get; }
        public string OnlyName { get; set; }
        public string OnlySchema { get; set; }

        public SqlServerViewObjectModel(SqlServerDatabaseObjectModel databse, string schema, string name)
        {
            Databse = databse;
            OnlyName = name;
            OnlySchema = schema;
            Name = schema + "." + name;
        }

        public override void LoadItems()
        {
        }

        public string[] GetObjectPath()
        {
            return new string[] { Databse.Server.Name, Databse.Name, OnlySchema, OnlyName };
        }

        public string[] GetObjectFields()
        {
            return new string[0];
        }
    }


    [DataContract]
    public class SqlServerConnectionModel : ReactiveObject
    {
        [DataMember]
        public string Alias { get; set; }

        [DataMember]
        public string Server { get; set; }
        [DataMember]
        public string UserId { get; set; }
        [DataMember]
        public string Password { get; set; }

        private SqlServerAuthentication _authentication = SqlServerAuthentication.Windows;
        [DataMember]
        public SqlServerAuthentication Authentication
        {
            get => _authentication;
            set => this.RaiseAndSetIfChanged(ref _authentication, value);
        }

        public SqlServerConnectorConfig ToConectorConfig()
        {
            return new SqlServerConnectorConfig()
            {
                Alias = this.Alias,
                Server = this.Server,
                Authentication = (Traficante.Connect.Connectors.SqlServerAuthentication)this.Authentication,
                UserId = this.UserId,
                Password = this.Password
            };
        }
    }

    public enum SqlServerAuthentication : int
    {
        Windows = 0,
        SqlServer = 1
    }
}
